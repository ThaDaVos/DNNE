using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DNNE.Source.Wrappings.AsyncToUnManaged;

[Generator]
public class AsyncToUnMangedGenerator : ISourceGenerator
{
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
            i.AddSource("AsyncUnmanagedCallersOnlyAttribute.g.cs", CodeFiles.ATTRIBUTE_ASYNC_UNMANAGED_CALLERS_ONLY_ATTRIBUTE);
            i.AddSource(
                "BridgingContext.g.cs", 
                Shared.CodeFiles.CLASS_BRIDGING_CONTEXT
                    .Substitute(new () {
                        {"namespace", "DNNE.Wrappings.AsyncToUnManaged"}
                    }, true)
            );
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