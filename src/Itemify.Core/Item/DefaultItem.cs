using System;
using System.Collections.Generic;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql.Entities;

namespace Itemify.Core.Item
{
    public class DefaultItem : ItemBase, IItemReference
    {
        private readonly IItemReference parent;
        public static IItemReference Root => new ItemReference(Guid.Empty, DefaultTypes.Root);

        public bool IsRoot => this.Guid == Guid.Empty && Type == DefaultTypes.Root;
        public bool HasUnknownType => Type == DefaultTypes.Unknown;

        public IReadOnlyCollection<IItemReference> Children => children;
        public IReadOnlyCollection<IItemReference> Related => related;


        public DefaultItem(string type)
            : base(new ItemEntity() { Guid = Guid.Empty, Type = type }, Root, true)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem()
            : base(new ItemEntity() { Guid = Guid.Empty, Type = DefaultTypes.Unknown }, Root, true)
        {
            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem(Guid newGuid)
            : base(new ItemEntity() { Guid = newGuid, Type = DefaultTypes.Unknown }, Root, true)
        {
            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem(Guid newGuid, string type)
            : base(new ItemEntity() { Guid = newGuid, Type = type }, Root, true)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem(Guid newGuid, string type, IItemReference parent) 
            : base(new ItemEntity() { Guid = newGuid, Type = type }, parent, true)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!IsRoot && parent == null) throw new ArgumentNullException(nameof(parent));

            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem(ItemEntity entity)
            : base(entity, new ItemReference(entity.ParentGuid, entity.ParentType), false)
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
