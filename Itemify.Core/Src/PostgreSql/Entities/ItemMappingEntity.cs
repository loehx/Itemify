﻿using System;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.ItemAccess.Entities
{
    class ItemMappingEntity : IAnonymousEntity
    {
        [PostgreSqlColumn("guid", indexing: PostgreSqlIndexType.Clustered)]
        public Guid Guid { get; set; }

        [PostgreSqlColumn("type")]
        public string Type { get; set; }

        [PostgreSqlColumn("t_guid")]
        public Guid TargetGuid { get; set; }

        [PostgreSqlColumn("t_type")]
        public string TargetType { get; set; }
    }
}