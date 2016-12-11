using System;
using Itemify.Core.ItemAccess.Entities;
using Itemify.Core.Typing;

namespace Itemify.Core.Item
{
    class DefaultItem : ItemBase
    {
        public DefaultItem(Guid newGuid, TypeItem type, ItemContext context, IItemReference parent) 
            : base(context, new ItemEntity(), parent)
        {
            Guid = newGuid;
            Type = type;
        }
    }
}
