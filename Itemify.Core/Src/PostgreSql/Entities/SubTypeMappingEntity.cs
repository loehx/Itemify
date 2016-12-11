using System;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.ItemAccess.Entities
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
