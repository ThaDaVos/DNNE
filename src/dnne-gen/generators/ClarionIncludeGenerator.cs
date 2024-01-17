using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using DNNE.Assembly;
using DNNE.Languages.Clarion;

namespace DNNE.Generators
{
    internal class ClarionIncludeGenerator : ClarionGenerator
    {
        internal const string METHOD_FORMAT = "{0} PROCEDURE({1}),{2}{3}";

        internal ClarionIncludeGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
        {
        }

        public override string ParseOutPutFileName(string outputFile)
        {
            return outputFile.Replace("g.c", "g.inc");
        }

        protected override void Write(Stream outputStream)
        {
            using var writer = new StreamWriter(outputStream);
            var extraCode = new StringBuilder();

            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                string className = ResolveClassName(enclosingType);

                writer.WriteLine(
                @$"{className} CLASS,TYPE,MODULE('{this.assemblyInformation.Name}.g.clw'),LINK('{this.assemblyInformation.Name}.g.clw')
instance LONG"
            );

                var wrapperBuilder = new StringBuilder();

                foreach (ExportedMethod export in enclosingType.ExportedMethods.OrderBy(ex => ex.MethodName))
                {
                    var safeTypeName = export.EnclosingTypeName.Replace("_", "").Replace(".", "_");

                    Dictionary<string, string> arguments = new ();

                    foreach (var argument in export.Arguments.Where(arg => arg.Index >= 0))
                    {
                        string type = argument.Type;

                        if (argument.Attributes.Any(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Argument"))
                        {
                            type = argument.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Argument").First().Value;
                        }

                        if (argument.Index == 0 && type == "intptr_t")
                        {
                            continue;
                        }

                        arguments.Add(argument.Name, type);

                        foreach (var attribute in argument.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "IncCode" && attr.Target == "Argument"))
                        {
                            extraCode.AppendLine(attribute.Value);
                        }
                    }

                    var returnType = ClarionTypeProvider.MapTypeToClarion(export.ReturnType);

                    if (export.Attributes.Any(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Return"))
                    {
                        returnType = export.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Return").First().Value;
                    }

                    foreach (var attribute in export.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "IncCode" && attr.Target == "Method"))
                    {
                        extraCode.AppendLine(attribute.Value);
                    }

                    switch (export.MethodName.ToUpper())
                    {
                        case "INIT":
                        case "CONSTRUCT":
                            wrapperBuilder.AppendLine($@"CONSTRUCT PROCEDURE()");
                            break;
                        case "DESTROY":
                        case "DESTRUCT":
                            wrapperBuilder.AppendLine($@"DESTRUCT PROCEDURE()");
                            break;
                        default:
                            string modifiers = string.Join(
                                ',',
                                new string[] {
                                    arguments.Keys.Count > 0 ? "PROC" : ""
                                }
                                .Where(s => !string.IsNullOrEmpty(s))
                            );

                            if (export.Attributes.Any(attr => attr.Group.Contains("MatrixMethod") && attr.Target == "Method"))
                            {
                                List<string> processedParameters = new (export.Attributes.Where(attr => attr.Group.Contains("MatrixMethod") && attr.Target == "Method").Count());
                                List<string> matrixedFormats = [METHOD_FORMAT];

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

                                    List<string> newMatrixedFormats = new(customAttributeTypedArguments.Length * matrixedFormats.Count);

                                    foreach (string format in matrixedFormats)
                                    {
                                        foreach (CustomAttributeTypedArgument<KnownType> customAttributeTypedArgument in customAttributeTypedArguments)
                                        {
                                            string value = customAttributeTypedArgument.Value as string;

                                            newMatrixedFormats.Add(
                                                format.Replace(
                                                    "{0}",
                                                    "{0}" + (connectByArgument.Value ?? "Of") + value.First().ToString().ToUpper() + value.Substring(1)
                                                )
                                            );
                                        }
                                    }

                                    matrixedFormats = newMatrixedFormats;
                                    processedParameters.Add(parameter);
                                }

                                foreach (string matrixedFormat in matrixedFormats.Where(format => format.Equals(METHOD_FORMAT) == false))
                                {
                                    wrapperBuilder.AppendLine(string.Format(
                                        matrixedFormat,
                                        export.MethodName,
                                        arguments
                                            .Where(arg => !processedParameters.Contains(arg.Key))
                                            .Select(arg => arg.Value)
                                            .AggregateSafely((a, b) => $"{a},{b}"),
                                        returnType,
                                        string.IsNullOrEmpty(modifiers) ? "" : $",{modifiers}"
                                    ));
                                }
                            }

                            wrapperBuilder.AppendLine(string.Format(
                                METHOD_FORMAT,
                                export.MethodName,
                                arguments.Values.AggregateSafely((a, b) => $"{a},{b}"),
                                returnType,
                                string.IsNullOrEmpty(modifiers) ? "" : $",{modifiers}"
                            ));
                            break;
                    }
                }
                writer.Write(wrapperBuilder);
                writer.WriteLine(@"                           END");
                writer.WriteLine();
                writer.Write(extraCode.ToString().Trim());
            }
        }
    }
}
