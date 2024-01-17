using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DNNE.Source.Wrappings.InstancedUnManaged;

public class Marker : ISyntaxContextReceiver
{
    private const string ATTRIBUTE = "DNNE.Wrappings.InstancedUnManaged.InstancedUnmanagedCallersOnlyAttribute";
    internal List<IMethodSymbol> Methods { get; } = new List<IMethodSymbol>();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax && methodDeclarationSyntax.AttributeLists.Count > 0)
        {
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax) as IMethodSymbol;

            if (
                methodSymbol != null
                && methodSymbol.IsStatic == false
                && methodSymbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == ATTRIBUTE)
            ) {
                this.Methods.Add(methodSymbol);
            }
        }
    }
}
