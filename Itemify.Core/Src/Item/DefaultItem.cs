using System;
using System.Collections.Generic;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Core.Typing;

namespace Itemify.Core.Item
{
    class DefaultItem : ItemBase, IItem
    {
        public IReadOnlyCollection<IItemReference> Children => children;
        public IReadOnlyCollection<IItemReference> Related => related;

        public DefaultItem(Guid newGuid, TypeItem type, ItemContext context, IItemReference parent) 
            : base(context, new ItemEntity(), parent, true)
        {
            Guid = newGuid;
            Type = type;
            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem(ItemEntity entity, ItemContext context)
            : base(context, entity, new ItemReference(entity.ParentGuid, context.TypeManager.ParseTypeItem(entity.ParentType)), false)
        {
            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        internal void AddChildren(IEnumerable<IItemReference> refs)
        {
            children.SetReadOnly(false);
            children.AddRange(refs);
            children.SetReadOnly(true);
        }

        internal void AddRelated(IEnumerable<IItemReference> refs)
        {
            related.SetReadOnly(false);
            related.AddRange(refs);
            related.SetReadOnly(true);
        }
    }
}
