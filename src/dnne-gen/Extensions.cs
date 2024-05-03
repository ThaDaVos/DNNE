using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Metadata;

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
            throw new ArgumentNullException(nameof(func));
        }

        return source.Any() ? source.Aggregate(func) : default;
    }
}