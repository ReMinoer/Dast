using System;
using System.Collections.Generic;
using System.Linq;

namespace Dast.Converters.Utils
{
    static public class UtilsExtensions
    {
        static public string Aggregate<T>(this IEnumerable<T> enumerable, Func<T, string> func)
        {
            return enumerable.Aggregate("", (current, next) => current + func(next));
        }

        static public string Aggregate(this IEnumerable<string> enumerable)
        {
            return enumerable.Aggregate("", (current, next) => current + next);
        }

        static public bool HasMultipleLine(this string text)
        {
            return text.Contains('\n');
        }

        static public bool Contains(this string text, params char[] characters)
        {
            return text.Any(t => characters.Any(c => t == c));
        }
    }
}