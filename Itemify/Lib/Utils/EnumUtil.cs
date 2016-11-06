using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Itemify.Lib.Utils
{
    internal static class EnumUtil
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
            if (!type.IsEnum)
                throw new ArgumentException($"Parameter {nameof(enumItem)} must be an enum. Actual: {type}");

            var itemName = enumItem.ToString();
            var field = type.GetField(itemName);
            return field.GetCustomAttributes(typeof(T), false).OfType<T>();
        }


        public static IEnumerable<TEnum> GetValues<TEnum>()
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
                throw new ArgumentException($"Parameter {nameof(TEnum)} must be an enum. Actual: {type}");

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                yield return (TEnum)field.GetValue(null);
            }
        }

        public static IEnumerable<Tuple<TEnum, TAttribute>> GetValuesHavingCustomAttribute<TEnum, TAttribute>()
            where TAttribute : Attribute
            where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
                throw new ArgumentException($"Parameter {nameof(TEnum)} must be an enum. Actual: {type}");

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<TAttribute>();
                if (attr != null)
                    yield return Tuple.Create((TEnum)field.GetValue(null), attr);
            }
        }
    }
}
