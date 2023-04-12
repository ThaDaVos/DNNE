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

                    for (int i = 0; i < export.ArgumentTypes.Length; i++)
                    {
                        var type = ClarionTypeProvider.MapTypeToClarion(export.ArgumentTypes[i]);
                        var name = export.ArgumentNames[i];

                        if (i == 0 && export.ArgumentTypes[i] == "intptr_t")
                        {
                            continue;
                        }

                        arguments += $",{type}";
                    }

                    arguments = arguments.Trim(',', ' ');

                    var returnType = ClarionTypeProvider.MapTypeToClarion(export.ReturnType);

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
