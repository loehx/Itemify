using System;

namespace Itemify.Core.PostgreSql.Entities
{

    internal class ItemEntity : IGloballyUniqueEntity
    {
        [PostgreSqlColumn(primaryKey: true)]
        public Guid Guid { get; set; }

        [PostgreSqlColumn]
        public string Type { get; set; }

        [PostgreSqlColumn]
        public Guid ParentGuid { get; set; }

        [PostgreSqlColumn]
        public string ParentType { get; set; }

        [PostgreSqlColumn()]
        public string Name { get; set; }

        [PostgreSqlColumn()]
        public double? ValueNumber { get; set; }

        [PostgreSqlColumn()]
        public DateTime? ValueDate { get; set; }

        [PostgreSqlColumn()]
        public string ValueJson { get; set; }

        [PostgreSqlColumn()]
        public string ValueJsonType { get; set; }

        [PostgreSqlColumn()]
        public string ValueString { get; set; }

        [PostgreSqlColumn()]
        public byte[] ValueBinary { get; set; }

        [PostgreSqlColumn]
        public int? Order { get; set; }

        [PostgreSqlColumn]
        public string SubTypes { get; set; }

        [PostgreSqlColumn]
        public DateTime Created { get; set; }

        [PostgreSqlColumn]
        public DateTime Modified { get; set; }

        [PostgreSqlColumn]
        public int Revision { get; set; }

        [PostgreSqlColumn]
        public bool Debug { get; set; }
    }
}
