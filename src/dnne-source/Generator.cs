using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DNNE.Source;

[Generator]
public class Generator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization((i) =>
        {
            i.AddSource("AsyncUnmanagedCallersOnlyAttribute.g.cs", CodeFiles.ATTRIBUTE_ASYNC_UNMANAGED_CALLERS_ONLY_ATTRIBUTE);
            i.AddSource("BridgingContext.g.cs", CodeFiles.CLASS_BRIDGING_CONTEXT);
        });

        context.RegisterForSyntaxNotifications(() => new Marker());
    }
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not Marker marker) return;

        foreach (IGrouping<INamedTypeSymbol, IMethodSymbol> group in marker.Methods.GroupBy<IMethodSymbol, INamedTypeSymbol>(m => m.ContainingType, SymbolEqualityComparer.Default))
        {
            if (group.Key == null) continue;

            string? source = new GeneratorSource(group.Key, group.ToList()).Generate();

            if (source == null) continue;

            context.AddSource($"{group.Key.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }
}