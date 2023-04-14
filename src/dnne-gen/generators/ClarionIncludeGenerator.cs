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
            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                string className = ResolveClassName(enclosingType);

                writer.WriteLine(
                @$"{className} CLASS,TYPE,MODULE('{this.assemblyInformation.Name}.g.clw'),LINK('{this.assemblyInformation.Name}.g.clw')
instance LONG"
            );

                var wrapperBuilder = new StringBuilder();

                foreach (var export in enclosingType.ExportedMethods.OrderBy(ex => ex.MethodName))
                {
                    var safeTypeName = export.EnclosingTypeName.Replace("_", "").Replace(".", "_");

                    var arguments = "";

                    foreach (var argument in export.Arguments)
                    {
                        string type = argument.Type;

                        if (argument.Attributes.Any(attr => attr.Group == "Type" && attr.Target == "Method"))
                        {
                            type = argument.Attributes.Where(attr => attr.Group == "Type" && attr.Target == "Method").First().Value;
                        }

                        if (argument.Index == 0 && type == "intptr_t") {
                            continue;
                        }

                        arguments += $",{type}";
                    }

                    arguments = arguments.Trim(',', ' ');

                    var returnType = ClarionTypeProvider.MapTypeToClarion(export.ReturnType);

                    if (export.UsedAttributes.Any(attr => attr.Group == "Type" && attr.Target == "Return"))
                    {
                        returnType = export.UsedAttributes.Where(attr => attr.Group == "Type" && attr.Target == "Return").First().Value;
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
            }
        }
    }
}
