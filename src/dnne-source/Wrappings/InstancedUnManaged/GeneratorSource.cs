using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DNNE.Source.Wrappings.InstancedUnManaged;

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
using System.Runtime.InteropServices;
namespace {namespaceName};
{classDefinition}
{{");

        foreach (IMethodSymbol method in methods)
        {
            source.Append(
                method.IsAsync
                    ? GenerateAsyncMethod(classSymbol.Name, method)
                    : GenerateSyncMethod(classSymbol.Name, method)
            );
        }

        source.Append(@"
}");

        return source.ToString();
    }

    private string GenerateSyncMethod(string parentType, IMethodSymbol method)
    {
        string returnType = method.ReturnType.ToDisplayString();
        string parameters = string.Join(", ", method.Parameters.Select(p => p.ToDisplayString()));

        string attributes = string
            .Join("\n", method.GetAttributes().Select(attribute => attribute.CreateCopyOfAttributeAsString()))
            .Replace(
                "DNNE.Wrappings.InstancedUnManaged.InstancedUnmanagedCallersOnlyAttribute",
                "System.Runtime.InteropServices.UnmanagedCallersOnly"
            );;

        return $@"
        {attributes}
        public static {returnType} {method.Name}(nint instancePointer, {parameters})
        {{
            {parentType}? instance = ({parentType}?)GCHandle.FromIntPtr(instancePointer).Target;

            if (instance == null) throw new System.ArgumentNullException(nameof(instance));

            return instance.{method.Name}({string.Join(", ", method.Parameters.Select(p => p.GetSafeguardedParameterName()))});
        }}";
    }
    private string GenerateAsyncMethod(string parentType, IMethodSymbol method)
    {
        string methodName = method.Name.EndsWith("Async") ? method.Name.Remove(method.Name.Length - 5) : $"{method.Name}Sync";
        string returnType = method.ReturnType.ToDisplayString();
        string parameters = string.Join(", ", method.Parameters.Select(p => p.ToDisplayString()));

        string attributes = string
            .Join("\n", method.GetAttributes().Select(attribute => attribute.CreateCopyOfAttributeAsString()))
            .Replace(
                "DNNE.Wrappings.InstancedUnManaged.InstancedUnmanagedCallersOnlyAttribute",
                "System.Runtime.InteropServices.UnmanagedCallersOnly"
            );

        returnType = returnType == "System.Threading.Tasks.Task"
                ? "void"
                : regex.Replace(returnType, @"$1", 1);

        return $@"
        {attributes}
        public static {returnType} {methodName}(nint instancePointer, {parameters})
        {{
            {parentType}? instance = ({parentType}?)GCHandle.FromIntPtr(instancePointer).Target;

            if (instance == null) throw new System.ArgumentNullException(nameof(instance));

            using var context = new DNNE.Wrappings.InstancedUnManaged.BridgingContext();

            return context.Run(() => {method.Name}({string.Join(", ", method.Parameters.Select(p => p.GetSafeguardedParameterName()))}));
        }}";
    }
}