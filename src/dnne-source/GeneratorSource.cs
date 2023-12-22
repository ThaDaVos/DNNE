using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DNNE.Source;

internal class GeneratorSource
{
    private Regex regex = new(@"(?>System\.)?(?>Threading\.)?(?>Tasks\.)?Task<([a-zA-Z]*)>");

    private INamedTypeSymbol classSymbol;
    private IList<IMethodSymbol> methods;

    public GeneratorSource(INamedTypeSymbol type, IList<IMethodSymbol> methods)
    {
        this.classSymbol = type;
        this.methods = methods;
    }

    internal string? Generate()
    {
        if (classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default) == false)
        {
            return null; //TODO: issue a diagnostic that it must be top level
        }

        string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

        string classDefinition =
            classSymbol switch
            {
                { DeclaredAccessibility: Accessibility.Public } => $"public",
                { DeclaredAccessibility: Accessibility.Internal } => $"internal",
                { DeclaredAccessibility: Accessibility.Private } => $"private",
                _ => throw new NotImplementedException()
            }
            + (classSymbol.IsStatic ? " static" : "")
            + (classSymbol.IsSealed ? " sealed" : "")
            + $" partial class {classSymbol.Name}"
        ;

        StringBuilder source = new($@"
#nullable enable
namespace {namespaceName};
{classDefinition}
{{
");

        foreach (IMethodSymbol method in methods)
        {
            source.Append(GenerateMethod(method));
        }

        source.Append(@"
}");

        return source.ToString();
    }

    private string GenerateMethod(IMethodSymbol method)
    {
        string methodName = method.Name.EndsWith("Async") ? method.Name.Remove(method.Name.Length - 5) : $"{method.Name}Sync";
        string returnType = method.ReturnType.ToDisplayString();
        string parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));

        string attributes = string
            .Join("\n", method.GetAttributes().Select(CreateCopyOfAttribute))
            .Replace(
                "AsyncToUnManaged.AsyncUnmanagedCallersOnly",
                "System.Runtime.InteropServices.UnmanagedCallersOnly"
            );

        returnType = returnType == "System.Threading.Tasks.Task"
                ? "void"
                : regex.Replace(returnType, @"$1", 1);

        return $@"
        {attributes}
        public static {returnType} {methodName}({parameters})
        {{
            using var context = new AsyncToUnManaged.BridgingContext();

            return context.Run(() => {method.Name}({string.Join(", ", method.Parameters.Select(p => p.Name))}));
        }}";
    }

    private string CreateCopyOfAttribute(AttributeData attribute)
    {
        string arguments = string.Join(", ", [
            ..attribute.ConstructorArguments.Select(a => a.ToCSharpString()),
            ..attribute.NamedArguments.Select(
                (KeyValuePair<string, TypedConstant> namedArgument) => {
                    string value = namedArgument.Value.ToCSharpString();
                    
                    if (namedArgument.Value.Kind == TypedConstantKind.Array)
                    {
                        value = $"new {namedArgument!.Value.Type.ToDisplayString()}{{{string.Join(", ", namedArgument.Value.Values.Select(v => v.ToCSharpString()))}}}";
                    }

                    return $"{namedArgument.Key} = {value}";
                }
            )
        ]);

        return $"[{attribute.AttributeClass?.ToDisplayString()}({arguments})]";
    }
}