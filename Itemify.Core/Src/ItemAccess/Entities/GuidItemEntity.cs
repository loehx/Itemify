using System;
using Itemify.Core.ItemAccess.Entities;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.Src.ItemAccess.Entities
{
    class GuidItemEntity : ItemEntityBase, IGloballyUniqueEntity
    {
        [PostgreSqlColumn(primaryKey: true)]
        public Guid Id { get; set; }
    }
}