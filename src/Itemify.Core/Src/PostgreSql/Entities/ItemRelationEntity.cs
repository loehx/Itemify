using System;

namespace Itemify.Core.PostgreSql.Entities
{
    class ItemRelationEntity : IAnonymousEntity
    {
        [PostgreSqlColumn("guid", indexing: PostgreSqlIndexType.Clustered)]
        public Guid Guid { get; set; }

        [PostgreSqlColumn("table_name")]
        public string Table { get; set; }

        [PostgreSqlColumn("t_guid")]
        public Guid TargetGuid { get; set; }

        [PostgreSqlColumn("t_table_name")]
        public string TargetTable { get; set; }
    }
}
