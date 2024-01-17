using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DNNE.Source.Attributes;

[Generator]
public class AttributesGenerator : ISourceGenerator
{
    internal readonly static string[] LANGUAGES = ["C99", "Clarion"];
    internal readonly static Dictionary<string, string> MATRIX_TYPES = new() {
        { "String", "string" },
        { "Integer", "int" }
    };

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUGGENERATOR
  if (!System.Diagnostics.Debugger.IsAttached)
  {
    System.Diagnostics.Debugger.Launch();
  }
#endif
        context.RegisterForPostInitialization((i) =>
        {
            foreach (string language in LANGUAGES)
            {
                i.AddSource(
                    hintName: string.Format(@"{0}DeclCodeAttribute.g.cs", language),
                    source: CodeFiles.ATTRIBUTE_DeclCode
                        .Substitute(new() { { "language", language } }, true)
                );
                i.AddSource(
                    hintName: string.Format(@"{0}TypeAttribute.g.cs", language),
                    source: CodeFiles.ATTRIBUTE_TypeCode
                        .Substitute(new() { { "language", language } }, true)
                );
                i.AddSource(
                    hintName: string.Format(@"{0}IncCodeAttribute.g.cs", language),
                    source: CodeFiles.ATTRIBUTE_IncludeCode
                        .Substitute(new() { { "language", language } }, true)
                );
                i.AddSource(
                    hintName: string.Format(@"{0}TypeContractAttribute.g.cs", language),
                    source: CodeFiles.ATTRIBUTE_TypeContractCode
                        .Substitute(new() { { "language", language } }, true)
                );
            }

            foreach (KeyValuePair<string, string> matrixType in MATRIX_TYPES)
            {
                i.AddSource(
                    hintName: string.Format(@"{0}MatrixMethodAttribute.g.cs", matrixType.Key),
                    source: CodeFiles.ATTRIBUTE_MatrixMethodCode
                        .Substitute(new() { { "language", matrixType.Key }, { "type", matrixType.Value } }, true)
                );
            }
        });
    }
    public void Execute(GeneratorExecutionContext context)
    {
    }
}