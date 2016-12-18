using System;

namespace Itemify.Core.PostgreSql.Entities
{
    class SubTypeMappingEntity : IAnonymousEntity
    {
        [PostgreSqlColumn(indexing: PostgreSqlIndexType.Clustered)]
        public string SubType { get; set; }

        [PostgreSqlColumn]
        public Guid Guid { get; set; }

        [PostgreSqlColumn]
        public string Type { get; set; }
    }
}
