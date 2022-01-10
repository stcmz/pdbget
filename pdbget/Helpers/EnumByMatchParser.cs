using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace pdbget.Helpers
{
    public static class EnumByMatchParser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]TEnum>
        where TEnum : notnull, Enum
    {
        static IEnumerable<TAttribute> GetAttributes<TAttribute>(TEnum enumValue)
        {
            return EnumAnnotationHelper<TEnum>.GetAttributes<TAttribute>(enumValue);
        }

        static readonly IDictionary<string, TEnum> _ciSynonymDict = EnumAnnotationHelper<TEnum>
            .Enums
            .SelectMany(o => GetAttributes<MatchAttribute>(o)
                .Where(p => !p.CaseSensitive && !p.UseRegex)
                .SelectMany(p => p.Patterns)
                .Select(p => (synonym: p.ToLower(), value: o))
                .Distinct()
            )
            .ToDictionary(o => o.synonym, o => o.value);

        static readonly IDictionary<string, TEnum> _csSynonymDict = EnumAnnotationHelper<TEnum>
            .Enums
            .SelectMany(o => GetAttributes<MatchAttribute>(o)
                .Where(p => p.CaseSensitive && !p.UseRegex)
                .SelectMany(p => p.Patterns)
                .Select(p => (synonym: p, value: o))
                .Distinct()
            )
            .ToDictionary(o => o.synonym, o => o.value);

        static readonly IList<(Regex regex, TEnum value)> _regexSynonymList = EnumAnnotationHelper<TEnum>
            .Enums
            .SelectMany(o => GetAttributes<MatchAttribute>(o)
                .Where(p => p.UseRegex)
                .SelectMany(p => p.Patterns.Select(q => (pattern: $"^{q.TrimStart('^').TrimEnd('$')}$", cs: p.CaseSensitive)))
                .Select(p => (regex: new Regex(p.pattern, RegexOptions.Compiled | (p.cs ? RegexOptions.None : RegexOptions.IgnoreCase)), value: o))
            )
            .ToArray();

        public static TEnum Parse(string s)
        {
            TryParse(s, out TEnum enumValue);
            return enumValue;
        }

        public static bool TryParse(string s, out TEnum enumValue)
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
                throw new Exception($"More than one enums of type {typeof(TEnum)} match '{s}'");

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
