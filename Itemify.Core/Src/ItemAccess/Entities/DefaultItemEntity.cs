using Itemify.Core.ItemAccess.Entities;
using Itemify.Core.PostgreSql;

namespace Itemify.Core.Src.ItemAccess.Entities
{
    class DefaultItemEntity : ItemEntityBase, IDefaultEntity
    {
        [PostgreSqlColumn(dataType: "SERIAL", primaryKey: true)]
        public int Id { get; set; }
    }
}