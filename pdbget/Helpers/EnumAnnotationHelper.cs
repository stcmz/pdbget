using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace pdbget.Helpers
{
    public static class EnumAnnotationHelper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)]TEnum>
        where TEnum : notnull, Enum
    {
        private static readonly IReadOnlyDictionary<TEnum, Attribute[]> _attributes =
            (from value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
             join field in typeof(TEnum).GetFields()
             on value.ToString() equals field.Name
             select new
             {
                 Value = value,
                 Attrs = field.GetCustomAttributes(false)
             }).ToDictionary(o => o.Value, o => o.Attrs.Cast<Attribute>().ToArray());

        private static readonly IReadOnlyDictionary<Type, TEnum[]> _enums = _attributes
            .SelectMany(o => o.Value, (o, i) => new { Type = i.GetType(), Value = o.Key })
            .GroupBy(o => o.Type)
            .ToDictionary(o => o.Key, o => o.Select(p => p.Value).ToArray());

        public static TAttribute GetAttribute<TAttribute>(TEnum enumValue)
        {
            return _attributes[enumValue].OfType<TAttribute>().First();
        }

        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(TEnum enumValue)
        {
            return _attributes[enumValue].OfType<TAttribute>();
        }

        public static bool HasAttribute<TAttribute>(TEnum enumValue)
        {
            return _attributes[enumValue].OfType<TAttribute>().Any();
        }

        public static IList<TEnum> GetWithAttribute<TAttribute>()
        {
            if (_enums.ContainsKey(typeof(TAttribute)))
                return _enums[typeof(TAttribute)];
            return new List<TEnum>();
        }

        public static readonly TEnum[] Enums = _attributes.Keys.ToArray();
    }
}
