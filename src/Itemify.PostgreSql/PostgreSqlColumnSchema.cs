using System;
using System.Reflection;
using Itemify.Core.PostgreSql.Util;

namespace Itemify.Core.PostgreSql
{
    public class PostgreSqlColumnSchema
    {
        private readonly PropertyInfo propertyInfo;
        private Func<object, object> parseFunc;

        public bool PrimaryKey { get; }

        public string Name { get; }

        public string DataType { get; }

        public bool Nullable { get; }

        public PostgreSqlIndexType Indexing { get; }

        public PostgreSqlColumnSchema(PostgreSqlColumnAttribute inner, PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            var underlyingType = System.Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            var type = underlyingType ?? propertyInfo.PropertyType;

            PrimaryKey = inner.PrimaryKey;
            Name = inner.Name ?? propertyInfo.Name;
            DataType = inner.DataType
                ?? (type == typeof(DateTimeOffset) ? "varchar(39)" : SqlUtil.GetSqlTypeFromType(type));
            Nullable = underlyingType != null || type.GetTypeInfo().IsClass;
            Indexing = inner.Indexing;
            // TODO: Implement column indexing

            this.parseFunc = getParseFuncByType(type, underlyingType != null);
        }

        public object GetValue(object entity)
        {
            var value = propertyInfo.GetValue(entity);

            // WORKAROUND: https://github.com/npgsql/EntityFramework6.Npgsql/issues/14
            if (value is DateTimeOffset)
                return ((DateTimeOffset) value).ToString("o"); 

            return value;
        }

        public void SetValue(object entity, object value)
        {
            if (!propertyInfo.CanWrite)
                return;

            if (DBNull.Value == value)
                value = null;

            value = parseFunc(value);
            propertyInfo.SetValue(entity, value);
        }

        private static Func<object, object> getParseFuncByType(Type type, bool nullable)
        {
            if (type == typeof(DateTimeOffset))
            {
                // WORKAROUND: https://github.com/npgsql/EntityFramework6.Npgsql/issues/14
                return o => DateTimeOffset.Parse((string) o);
            }

            return o => o;
        }
    }
}
