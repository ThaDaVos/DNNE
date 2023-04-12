using System.IO;
using System.Linq;
using System.Text;
using DNNE.Assembly;
using DNNE.Languages.Clarion;

namespace DNNE.Generators
{
    internal class ClarionCodeGenerator : ClarionGenerator
    {
        public ClarionCodeGenerator(AssemblyInformation assemblyInformation) : base(assemblyInformation)
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

            foreach (var enclosingType in this.assemblyInformation.ExportedTypes)
            {
                var className = ResolveClassName(enclosingType);

                foreach (var export in enclosingType.ExportedMethods.OrderBy(ex => ex.MethodName))
                {
                    var safeTypeName = export.EnclosingTypeName.Replace("_", "").Replace(".", "_");

                    var argumentDefinition = "";
                    var arguments = "";
                    var argumentNames = "";

                    for (int i = 0; i < export.ArgumentTypes.Length; i++)
                    {
                        var type = ClarionTypeProvider.MapTypeToClarion(export.ArgumentTypes[i]);
                        var name = export.ArgumentNames[i];

                        if (i == 0 && export.ArgumentTypes[i] == "intptr_t")
                        {
                            argumentNames += $"SELF.instance";
                            argumentDefinition += $"LONG";
                        }
                        else
                        {
                            argumentDefinition += $",{type}";
                            arguments += $",{type} {name}";
                            argumentNames += $", {name}";
                        }
                    }

                    argumentDefinition = argumentDefinition.Trim(',', ' ');
                    arguments = arguments.Trim(',', ' ');
                    argumentNames = argumentNames.Trim(',', ' ');

                    var returnType = ClarionTypeProvider.MapTypeToClarion(export.ReturnType);

                    moduleBuilder.AppendLine($@"        {safeTypeName}_{export.MethodName}({argumentDefinition}),{returnType},c,raw,dll(1),name('{className}_{export.MethodName}')");

                    switch (export.MethodName.ToUpper())
                    {
                        case "INIT":
                        case "CONSTRUCT":
                            string constructArgs = ClarionTypeProvider.ReplaceTypesWithDefaults(argumentDefinition);

                            wrapperBuilder.AppendLine($@"{className}.CONSTRUCT PROCEDURE()
    CODE
        SELF.instance = {safeTypeName}_{export.MethodName}({constructArgs});");
                            break;
                        case "DESTROY":
                        case "DESTRUCT":
                            string destructArgs = ClarionTypeProvider.ReplaceTypesWithDefaults(argumentDefinition);

                            wrapperBuilder.AppendLine($@"{className}.DESTRUCT PROCEDURE()
    CODE
        {safeTypeName}_{export.MethodName}({destructArgs});");
                            break;
                        default:
                            wrapperBuilder.AppendLine($@"{className}.{export.MethodName} PROCEDURE({arguments})!,{returnType}
    CODE
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
    }
}
