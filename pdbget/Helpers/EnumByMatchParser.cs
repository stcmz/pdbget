using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace pdbget.Helpers
{
    public static class EnumByMatchParser<T>
        where T : notnull, Enum
    {
        static IEnumerable<TAttribute> GetAttributes<TAttribute>(T enumValue)
        {
            return EnumAnnotationHelper<T>.GetAttributes<TAttribute>(enumValue);
        }

        static readonly IDictionary<string, T> _ciSynonymDict = EnumAnnotationHelper<T>
            .Enums
            .SelectMany(o => GetAttributes<MatchAttribute>(o)
                .Where(p => !p.CaseSensitive && !p.UseRegex)
                .SelectMany(p => p.Patterns)
                .Select(p => (synonym: p.ToLower(), value: o))
                .Distinct()
            )
            .ToDictionary(o => o.synonym, o => o.value);

        static readonly IDictionary<string, T> _csSynonymDict = EnumAnnotationHelper<T>
            .Enums
            .SelectMany(o => GetAttributes<MatchAttribute>(o)
                .Where(p => p.CaseSensitive && !p.UseRegex)
                .SelectMany(p => p.Patterns)
                .Select(p => (synonym: p, value: o))
                .Distinct()
            )
            .ToDictionary(o => o.synonym, o => o.value);

        static readonly IList<(Regex regex, T value)> _regexSynonymList = EnumAnnotationHelper<T>
            .Enums
            .SelectMany(o => GetAttributes<MatchAttribute>(o)
                .Where(p => p.UseRegex)
                .SelectMany(p => p.Patterns.Select(q => (pattern: $"^{q.TrimStart('^').TrimEnd('$')}$", cs: p.CaseSensitive)))
                .Select(p => (regex: new Regex(p.pattern, RegexOptions.Compiled | (p.cs ? RegexOptions.None : RegexOptions.IgnoreCase)), value: o))
            )
            .ToArray();

        public static T Parse(string s)
        {
            TryParse(s, out T enumValue);
            return enumValue;
        }

        public static bool TryParse(string s, out T enumValue)
        {
            if (s == null)
            {
                enumValue = default!;
                return false;
            }

            s = s.Trim();
            if (_csSynonymDict.TryGetValue(s, out enumValue!))
                return true;

            if (_ciSynonymDict.TryGetValue(s.ToLower(), out enumValue!))
                return true;

            var m = _regexSynonymList.Where(o => o.regex.IsMatch(s)).ToArray();

            if (m.Length > 1)
                throw new Exception($"More than one enums of type {typeof(T)} match '{s}'");

            if (m.Length == 1)
            {
                enumValue = m[0].value;
                return true;
            }

            enumValue = default!;
            return false;
        }

    }
}
