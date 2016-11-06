using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Itemify.Core.PostgreSql.Util
{
    public static class ReflectionUtil
    {
        internal static IEnumerable<T> Objectify<T>(IEnumerable<string> propNames, IEnumerable<IEnumerable<object>> dataSets)
            where T: new()
        {
            var type = typeof(T);
            var propertes = propNames
                .Select(k => type.GetProperty(k, BindingFlags.Public | BindingFlags.IgnoreCase))
                .ToArray();

            foreach (var dataSet in dataSets)
            {
                var obj = new T();

                foreach (var prop in propertes.InnerJoin(dataSet))
                {
                    prop.Item1.SetValue(obj, prop.Item2);
                }

                yield return obj;
            }
        }


        private static readonly Hashtable columnSchemata = Hashtable.Synchronized(new Hashtable());
        public static IReadOnlyList<PostgreSqlColumnSchema> GetColumnSchemas(Type type)
        {
            var cached = columnSchemata[type.GUID] as List<PostgreSqlColumnSchema>;
            if (cached != null)
                return cached;

            var properties = type.GetProperties();
            var results = new List<PostgreSqlColumnSchema>(properties.Length);

            for (int i = 0; i < properties.Length; i++)
            {
                var propertyInfo = properties[i];
                var attr = propertyInfo.GetCustomAttribute<PostgreSqlColumnAttribute>();
                if (attr == null)
                    continue;

                results.Add(new PostgreSqlColumnSchema(attr, propertyInfo));
            }

            columnSchemata[type.GUID] = results;
            return results;
        }
    }
}
