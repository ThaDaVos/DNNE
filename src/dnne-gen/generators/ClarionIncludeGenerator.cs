using System.IO;
using System.Linq;
using System.Text;
using DNNE.Assembly;
using DNNE.Languages.Clarion;

namespace DNNE.Generators
{
    internal class ClarionIncludeGenerator : ClarionGenerator
    {
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

                    var arguments = "";

                    foreach (var argument in export.Arguments.Where(arg => arg.Index >= 0))
                    {
                        string type = argument.Type;

                        if (argument.Attributes.Any(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Argument"))
                        {
                            type = argument.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "Type" && attr.Target == "Argument").First().Value;
                        }

                        if (argument.Index == 0 && type == "intptr_t") {
                            continue;
                        }

                        arguments += $",{type}";

                        foreach (var attribute in argument.Attributes.Where(attr => attr.TargetLanguage == "Clarion" && attr.Group == "IncCode" && attr.Target == "Argument"))
                        {
                            extraCode.AppendLine(attribute.Value);
                        }
                    }

                    arguments = arguments.Trim(',', ' ');

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
                            if (arguments.Length == 0)
                            {
                                wrapperBuilder.AppendLine($@"{export.MethodName} PROCEDURE({arguments}),{returnType}");
                            }
                            else
                            {
                                wrapperBuilder.AppendLine($@"{export.MethodName} PROCEDURE({arguments}),{returnType},PROC");
                            }
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
