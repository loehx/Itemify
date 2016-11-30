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
            DataType = inner.DataType ?? SqlUtil.GetSqlTypeFromType(type);
            Nullable = underlyingType != null || type.IsClass;
            Indexing = inner.Indexing;
            // TODO: Implement column indexing

            this.parseFunc = GetParseFuncByType(type, underlyingType != null);
        }

        public object GetValue(object entity)
        {
            return propertyInfo.GetValue(entity);
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

        public static Func<object, object> GetParseFuncByType(Type type, bool nullable)
        {
            if (type == typeof(DateTimeOffset))
            {
                return o =>
                    o is DateTime
                        ? new DateTimeOffset(DateTime.SpecifyKind((DateTime) o, DateTimeKind.Local))
                        : o;
            }

            return o => o;
        }
    }
}
