using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.Src.ItemAccess.Entities
{
    class SubTypeMappingEntity : IAnonymousEntity
    {
        [PostgreSqlColumn(indexing: PostgreSqlIndexType.Clustered)]
        public string SubType { get; set; }

        [PostgreSqlColumn]
        public Guid Guid { get; set; }

        [PostgreSqlColumn]
        public int Id { get; set; }

        [PostgreSqlColumn]
        public string Type { get; set; }
    }
}
