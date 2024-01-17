using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DNNE.Source;

public static class ArrayExtensions
{
    public static string Join<T>(this IEnumerable<T> source, string separator, Func<T, string> selector)
    {
        return string.Join(separator, source.Select(selector));
    }
    public static string Join<T>(this IEnumerable<T> source, string separator)
    {
        return string.Join(separator, source);
    }

}

public static class StringSubstituteExtension
{
    private static readonly Regex NamedReplacerPattern = new(@"(?<!\{)\{%(\w+)([^\}]*)%\}");
    private static readonly Regex OpeningBracketPattern = new(@"(?<!{){([^\d])(?<!{)");
    private static readonly Regex ClosingBracketPattern = new(@"(?!})([^\d])}(?!})");

    /// <summary>
    /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified dictionary.
    /// </summary>
    public static string Substitute(this string template, Dictionary<string, object> dictionary, bool escapeCurlyBrackets = false)
    {
        return Substitute(template, null, dictionary, escapeCurlyBrackets);
    }

    /// <summary>
    /// Replaces the format item in a specified string with the string representation of a corresponding object in a specified dictionary.
    /// A specified parameter supplies culture-specific formatting information.
    /// </summary>
    public static string Substitute(this string template, IFormatProvider? formatProvider, Dictionary<string, object> dictionary, bool escapeCurlyBrackets = false)
    {
        Dictionary<string, int>? map = new();

        List<object>? list = new();

        string? format = NamedReplacerPattern.Replace(
            template,
            match =>
                {
                    var name = match.Groups[1].Captures[0].Value;
                    if (!map.ContainsKey(name))
                    {
                        map[name] = map.Count;
                        list.Add(dictionary.ContainsKey(name) ? dictionary[name] : "");
                    }
                    return "{" + map[name] + match.Groups[2].Captures[0].Value + "}";
                }
            );

        if (escapeCurlyBrackets)
        {
            format = OpeningBracketPattern.Replace(format, "{{$1");
            format = ClosingBracketPattern.Replace(format, "$1}}");
        }

        return formatProvider == null
            ? string.Format(format, [.. list])
            : string.Format(formatProvider, format, [.. list]);
    }
}

public static class IParameterSymbolExtensions
{
    public static string GetSafeguardedParameterName(this IParameterSymbol symbol)
        => symbol.ToDisplayString().IndexOf('@') > -1  ? "@" + symbol.Name : symbol.Name;
}

public static class AttributeDataExtensions
{
    public static string CreateCopyOfAttributeAsString(this AttributeData attribute)
    {
        string[] argumentsArray = [
            ..attribute
                .ConstructorArguments
                    .Select((TypedConstant constructorArgument) => constructorArgument.ToAttributeArgumentValue()),
            ..attribute
                .NamedArguments
                    .Select((KeyValuePair<string, TypedConstant> namedArgument) => namedArgument.ToAttributeArgument())
        ];

        string arguments = argumentsArray.Join(", ");

        return $"[{attribute.AttributeClass?.ToDisplayString()}({arguments})]";
    }
}

public static class TypedConstantExtensions
{
    public static string ToAttributeArgument(this KeyValuePair<string, TypedConstant> pair)
        => $"{pair.Key} = {pair.Value.ToAttributeArgumentValue()}";

    public static string ToAttributeArgumentValue(this TypedConstant constant)
    {
        if (constant.Kind == TypedConstantKind.Array)
        {
            return $"new {constant!.Type!.ToDisplayString()}{{{string.Join(", ", constant.Values.Select(v => v.ToCSharpString()))}}}";
        }

        return constant.ToCSharpString();
    }
}