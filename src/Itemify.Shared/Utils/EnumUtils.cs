using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Itemify.Shared.Attributes;
// ReSharper disable once CheckNamespace
namespace Itemify.Shared.Utils
{
    public static class EnumUtil
    {
        public static T GetCustomAttribute<T>(object enumItem)
            where T : Attribute
        {
            var attr = TryGetCustomAttribute<T>(enumItem);
            if (attr == null)
                throw new Exception($"Attribute of type '{typeof(T).Name}' is missing in enum '{enumItem.GetType().Name}' on entry: '{enumItem}'");

            return attr;
        }

        public static T TryGetCustomAttribute<T>(object enumItem)
            where T : Attribute
        {
            return GetCustomAttributes<T>(enumItem).FirstOrDefault();
        }


        public static IEnumerable<T> GetCustomAttributes<T>(object enumItem)
            where T : Attribute
        {
            if (enumItem == null) throw new ArgumentNullException(nameof(enumItem));

            var type = enumItem.GetType();
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException($"Parameter {nameof(enumItem)} must be an enum. Actual: {type}");

            var itemName = enumItem.ToString();
            var field = type.GetTypeInfo().GetField(itemName);
            return field.GetCustomAttributes(typeof(T), false).OfType<T>();
        }


        public static IEnumerable<TEnum> GetValues<TEnum>()
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException($"Parameter {nameof(TEnum)} must be an enum. Actual: {type}");

            // Flagged enums
            if (type.GetTypeInfo().IsDefined(typeof(FlagsAttribute), false))
            {
                foreach (var e in GetFlagValues(type))
                    yield return (TEnum) (object) e;
            }
            else
            {
                var fields = type.GetTypeInfo().GetFields();
                foreach (var field in fields)
                {
                    if (field.IsSpecialName)
                        continue;

                    yield return (TEnum)field.GetValue(null);
                }
            }
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                var bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    yield return value;

                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        public static IEnumerable<Tuple<TEnum, TAttribute>> GetValuesHavingCustomAttribute<TEnum, TAttribute>()
            where TAttribute : Attribute
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.GetTypeInfo().IsEnum)
                throw new ArgumentException($"Parameter {nameof(TEnum)} must be an enum. Actual: {type}");

            var fields = type.GetTypeInfo().GetFields();
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<TAttribute>();
                if (attr != null)
                    yield return Tuple.Create((TEnum)field.GetValue(null), attr);
            }
        }

        public static string GetAlias<TEnum>(TEnum enumItem)
            where TEnum : struct
        {
            return GetCustomAttribute<AliasAttribute>(enumItem).Alias;
        }

        public static string TryGetAlias<TEnum>(TEnum enumItem)
            where TEnum : struct 
        {
            return TryGetCustomAttribute<AliasAttribute>(enumItem)?.Alias;
        }

        public static TEnum GetValueByAlias<TEnum>(string alias, bool ignoreCase = false)
            where TEnum : struct 
        {
            if (alias == null) throw new ArgumentNullException(nameof(alias));

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var result = GetValuesHavingCustomAttribute<TEnum, AliasAttribute>()
                    .Where(k => alias.Equals(k.Item2.Alias, comparison) || k.Item2.Alternatives.Contains(alias, comparer))
                    .Select(k => k.Item1)
                    .Take(1)
                    .ToArray();

            if (result.Length == 0)
                throw new Exception($"Enum value in {typeof(TEnum)} could not be found by alias: '{alias}'");

            return result[0];
        }

        public static TEnum TryGetValueByAlias<TEnum>(string alias, bool ignoreCase = false)
            where TEnum : struct
        {
            if (alias == null) return default(TEnum);

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return GetValuesHavingCustomAttribute<TEnum, AliasAttribute>()
                .Where(k => alias.Equals(k.Item2.Alias, comparison))
                .Select(k => k.Item1)
                .FirstOrDefault();
        }
    }
}
