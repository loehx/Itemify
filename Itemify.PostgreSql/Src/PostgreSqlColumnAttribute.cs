using System;

namespace Itemify.Core.PostgreSql
{
    public class PostgreSqlColumnAttribute : Attribute
    {
        public bool PrimaryKey { get; }
        public object DefaultValue { get; }
        public string Name { get; }
        public string DataType { get; }

        public PostgreSqlColumnAttribute()
        {
        }

        public PostgreSqlColumnAttribute(string name = null, string dataType = null, bool primaryKey = false, object defaultValue = null)
        {
            Name = name;
            DataType = dataType;
            PrimaryKey = primaryKey;
            DefaultValue = defaultValue;
        }
    }
}