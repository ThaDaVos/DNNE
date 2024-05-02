using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using DNNE.Assembly;
using DNNE.Languages.Clarion;

namespace DNNE.Generators
{
    internal class ClarionCodeGenerator : ClarionGenerator
    {
        internal ClarionCodeGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
        {
        }

        public override string ParseOutPutFileName(string outputFile)
        {
            return outputFile.Replace("g.c", "g.clw");
        }

        protected override void Write(Stream outputStream)
        {
            using var writer = new StreamWriter(outputStream);
            writer.WriteLine(
@$"MEMBER()
                    MAP
    module('{this.assemblyInformation.Name}NE')"
);

            var moduleBuilder = new StringBuilder();
            var wrapperBuilder = new StringBuilder();

            moduleBuilder.AppendLine($@"        {this.assemblyInformation.SafeName}_tryLoadRuntime(),SHORT,c,raw,dll(1),name('_{this.assemblyInformation.Name}_try_preload_runtime@0')");

            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                var className = ResolveClassName(enclosingType);

                foreach (var export in enclosingType.ExportedMethods.OrderBy(ex => ex.MethodName))
                {
                    var safeTypeName = export.EnclosingTypeName.Replace("_", "").Replace(".", "_");

                    var argumentDefinition = "";
                    var arguments = "";
                    var argumentNames = "";

                    foreach (var argument in export.Arguments.Where(arg => arg.Index >= 0))
                    {
                        string type = argument.Type;

                        if (argument.Attributes.Any(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Argument"))
                        {
                            type = argument.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Argument").First().Value;
                        }

                        if (argument.Index == 0 && type == "intptr_t")
                        {
                            argumentNames += $"SELF.instance";
                            argumentDefinition += $"LONG";
                        }
                        else
                        {
                            argumentDefinition += $",{type}";
                            arguments += $",{type} {argument.Name}";
                            argumentNames += $", {argument.Name}";
                        }
                    }

                    argumentDefinition = argumentDefinition.Trim(',', ' ');
                    arguments = arguments.Trim(',', ' ');
                    argumentNames = argumentNames.Trim(',', ' ');

                    var returnType = ClarionTypeProvider.MapTypeToClarion(export.ReturnType);

                    if (export.Attributes.Any(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Return"))
                    {
                        returnType = export.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Return").First().Value;
                    }

                    var defaultReturnValue = GetDefaultValueFor(returnType);

                    moduleBuilder.AppendLine($@"        {safeTypeName}_{export.MethodName}({argumentDefinition}),{returnType},c,raw,dll(1),name('{className}_{export.MethodName}')");

                    switch (export.MethodName.ToUpper())
                    {
                        case "INIT":
                        case "CONSTRUCT":
                            string constructArgs = ClarionTypeProvider.ReplaceTypesWithDefaults(argumentDefinition);

                            wrapperBuilder.AppendLine($@"{className}.CONSTRUCT PROCEDURE()
    CODE
        IF {this.assemblyInformation.SafeName}_tryLoadRuntime() <> 0
            RETURN;
        END

        SELF.instance = {safeTypeName}_{export.MethodName}({constructArgs});");
                            break;
                        case "DESTROY":
                        case "DESTRUCT":
                            string destructArgs = ClarionTypeProvider.ReplaceTypesWithDefaults(argumentDefinition);

                            wrapperBuilder.AppendLine($@"{className}.DESTRUCT PROCEDURE()
    CODE
        IF SELF.instance = 0
            RETURN;
        END
        
        {safeTypeName}_{export.MethodName}(SELF.instance);");
                            break;
                        default:
                            if (export.Attributes.Any(attr => attr.Group.Contains("MatrixMethod") && attr.Target == "Method"))
                            {
                                List<string> matrixedMethodNames = ["{0}"];
                                Dictionary<string, string> matrixedArgumentsByMethodName = new()
                                {
                                    ["{0}"] = arguments,
                                };
                                Dictionary<string, string> matrixedArgumentNamesByMethodName = new()
                                {
                                    ["{0}"] = argumentNames,
                                };

                                foreach (UsedAttribute attribute in export.Attributes.Where(attr => attr.Group.Contains("MatrixMethod") && attr.Target == "Method"))
                                {
                                    attribute.Values.TryGetValue("parameter", out AttributeArgument parameterArgument);
                                    attribute.Values.TryGetValue("values", out AttributeArgument valuesArgument);
                                    attribute.Values.TryGetValue("ConnectBy", out AttributeArgument connectByArgument);

                                    string parameter = (parameterArgument.Value as string).Replace("@", "");

                                    ImmutableArray<CustomAttributeTypedArgument<KnownType>> customAttributeTypedArguments = (ImmutableArray<CustomAttributeTypedArgument<KnownType>>)valuesArgument.Value;

                                    if (customAttributeTypedArguments == null)
                                    {
                                        Console.WriteLine("No values found in matrix method attribute. {0} | {1}", valuesArgument.Type.ToString(), valuesArgument.Value);
                                        continue;
                                    }

                                    List<string> newMatrixedMethodNames = new(customAttributeTypedArguments.Length * matrixedMethodNames.Count);

                                    foreach (string format in matrixedMethodNames)
                                    {
                                        foreach (CustomAttributeTypedArgument<KnownType> customAttributeTypedArgument in customAttributeTypedArguments)
                                        {
                                            string value = customAttributeTypedArgument.Value as string;

                                            string newFormat = format.Replace(
                                                "{0}",
                                                "{0}" + (connectByArgument.Value ?? "Of") + value.First().ToString().ToUpper() + value.Substring(1)
                                            );

                                            newMatrixedMethodNames.Add(
                                                newFormat
                                            );

                                            if (matrixedArgumentsByMethodName.ContainsKey(newFormat) == false)
                                            {
                                                matrixedArgumentsByMethodName.Add(
                                                    newFormat,
                                                    matrixedArgumentsByMethodName[format]
                                                        .Split(',')
                                                        .Where(arg => arg.Contains(parameter) == false)
                                                        .Aggregate((a, b) => $"{a},{b}")
                                                        .Trim(',', ' ')
                                                );
                                            }
                                            else
                                            {
                                                matrixedArgumentsByMethodName[newFormat] = matrixedArgumentsByMethodName[newFormat]
                                                    .Split(',')
                                                    .Where(arg => arg.Contains(parameter) == false)
                                                    .Aggregate((a, b) => $"{a},{b}")
                                                    .Trim(',', ' ');
                                            }

                                            if (matrixedArgumentNamesByMethodName.ContainsKey(newFormat) == false)
                                            {
                                                matrixedArgumentNamesByMethodName.Add(newFormat, matrixedArgumentNamesByMethodName[format].Replace(parameter, $"'{value}'"));
                                            }
                                            else
                                            {
                                                matrixedArgumentNamesByMethodName[newFormat] = matrixedArgumentNamesByMethodName[newFormat].Replace(parameter, $"'{value}'");
                                            }
                                        }
                                    }

                                    matrixedMethodNames = newMatrixedMethodNames;
                                }

                                Console.WriteLine(string.Join('|', matrixedMethodNames));

                                foreach (string matrixedMethodName in matrixedMethodNames.Where(format => format.Equals(export.MethodName) == false))
                                {
                                    wrapperBuilder.AppendLine($@"{className}.{string.Format(matrixedMethodName, export.MethodName)} PROCEDURE({matrixedArgumentsByMethodName[matrixedMethodName]})!,{returnType}
    CODE
        IF SELF.instance = 0
            RETURN {defaultReturnValue};
        END
        
        RETURN {safeTypeName}_{export.MethodName}({matrixedArgumentNamesByMethodName[matrixedMethodName]});");
                                }
                            }

                            wrapperBuilder.AppendLine($@"{className}.{export.MethodName} PROCEDURE({arguments})!,{returnType}
    CODE
        IF SELF.instance = 0
            RETURN {defaultReturnValue};
        END
        
        RETURN {safeTypeName}_{export.MethodName}({argumentNames});");
                            break;
                    }
                    wrapperBuilder.AppendLine();
                }
            }

            writer.Write(moduleBuilder);

            writer.WriteLine(@"    END");
            writer.WriteLine(@"                    END");
            writer.WriteLine(@"");
            writer.WriteLine(@$"  include('{this.assemblyInformation.Name}.g.inc'),ONCE");
            writer.WriteLine(@"");
            writer.Write(wrapperBuilder);
        }

        protected string GetDefaultValueFor(string type) => type.ToUpper().Trim() switch
        {
            "LONG" or "SHORT" or "REAL" => "-1",
            "BOOL" => "FALSE",
            _ => "'Error: DotNet not loaded'",
        };
    }
}
