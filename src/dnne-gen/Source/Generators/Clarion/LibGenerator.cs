using System.Diagnostics.CodeAnalysis;
using DNNE.Assembly;
using DNNE.Assembly.Entities;
using DNNE.Assembly.Entities.Interfaces;
using DNNE.Source.IO;
using DNNE.Source.Naming;

namespace DNNE.Source.Generators.Clarion;

internal class LibGenerator : AbstractCodeGenerator
{
    public LibGenerator(AssemblyInformation assemblyInformation, INamingHelper namingHelper) : base(assemblyInformation, namingHelper)
    {
    }

    protected override bool HandleClassClosing(SourceWriter writer, IExportedType type)
    {
        throw new System.NotImplementedException();
    }

    protected override bool HandleClassOpening(SourceWriter writer, IExportedType type)
    {
        throw new System.NotImplementedException();
    }

    protected override bool HandleField(SourceWriter writer, ExportedField field)
    {
        throw new System.NotImplementedException();
    }

    protected override bool HandleInclusionOfSourceFile(SourceWriter writer, string sourceFileName)
    {
        throw new System.NotImplementedException();
    }

    protected override bool HandleMethod(SourceWriter writer, IExportedMethod method)
    {
        throw new System.NotImplementedException();
    }

    protected override bool HandleProperty(SourceWriter writer, ExportedProperty property)
    {
        throw new System.NotImplementedException();
    }

    protected override string ResolveFileName()
    {
        throw new System.NotImplementedException();
    }

    protected override bool WriteCommentToSourceFile(SourceWriter writer, SourceGeneratorCommentStyle style, [StringSyntax("CompositeFormat")] string format, params object?[] arg)
    {
        throw new System.NotImplementedException();
    }
}
