using System;

namespace Itemify.Core.PostgreSql
{
    public class PostgreSqlColumnAttribute : Attribute
    {
        public bool PrimaryKey { get; }
        public object DefaultValue { get; }
        public string Name { get; }
        public string DataType { get; }
        public PostgreSqlIndexType Indexing { get; }

        public PostgreSqlColumnAttribute(string name = null, string dataType = null, bool primaryKey = false, object defaultValue = null, PostgreSqlIndexType indexing = PostgreSqlIndexType.Clustered)
        {
            Indexing = indexing;
            Name = name;
            DataType = dataType;
            PrimaryKey = primaryKey;
            DefaultValue = defaultValue;
        }
    }
}