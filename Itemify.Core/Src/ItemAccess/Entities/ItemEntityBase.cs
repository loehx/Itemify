using System;
using System.Collections.Generic;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.ItemAccess.Entities
{

    public class ItemEntityBase
    {
        [PostgreSqlColumn]
        public string Type { get; set; }

        [PostgreSqlColumn]
        public int ParentId { get; set; }

        [PostgreSqlColumn]
        public int Level { get; set; }

        [PostgreSqlColumn()]
        public double? ValueNumber { get; set; }

        [PostgreSqlColumn()]
        public double? ValueDate { get; set; }

        [PostgreSqlColumn()]
        public string ValueDynamic { get; set; }

        [PostgreSqlColumn()]
        public string ValueString { get; set; }

        [PostgreSqlColumn(dataType: "varchar(256)")]
        public string ValueDynamicType { get; set; }

        [PostgreSqlColumn()]
        public byte[] ValueBinary { get; set; }

        [PostgreSqlColumn]
        public int? Order { get; set; }

        [PostgreSqlColumn]
        public IReadOnlyList<string> SubTypes { get; set; }

        [PostgreSqlColumn]
        public IReadOnlyList<string> ChildrenTypes { get; set; }

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
