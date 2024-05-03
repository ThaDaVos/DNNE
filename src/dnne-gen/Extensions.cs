using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace DNNE;

internal static class Extensions
{
    internal static TSource? AggregateSafely<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (func == null)
        {
            throw new ArgumentNullException(paramName: nameof(func));
        }

        return source.Any() ? source.Aggregate(func) : default;
    }

    internal static bool IsMatched(this Regex regex, string input, out Match? match)
    {
        if (regex == null)
        {
            throw new ArgumentNullException(nameof(regex));
        }

        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        match = regex.Match(input);
        
        return match.Success;
    }
}