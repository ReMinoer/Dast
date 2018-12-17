using System;
using System.Collections.Generic;
using System.Linq;

namespace Dast.Outputs.Base
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
            return text.Cast<char>().Contains('\n') && !text.EndsWith("\n");
        }

        static public bool ContainsAny(this string text, params char[] characters)
        {
            return text.Cast<char>().Any(c => characters.Any(character => character == c));
        }

        static public bool EqualsOrdinal(this string a, string b)
        {
            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }
    }
}