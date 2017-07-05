using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.PostgreSql.Entities;

namespace Itemify.Core.Item
{
    public class DefaultItem : ItemBase
    {
        public static DefaultItemReference Root => new DefaultItemReference(Guid.Empty, DefaultTypes.Root);

        public bool IsRoot => this.Guid == Guid.Empty && Type == DefaultTypes.Root;
        public bool HasUnknownType => Type == DefaultTypes.Unknown;

        public IReadOnlyCollection<DefaultItem> Children => children.Cast<DefaultItem>().ToArray();
        public IReadOnlyCollection<DefaultItem> Related => related.Cast<DefaultItem>().ToArray();


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

        public DefaultItem(Guid newGuid, string type, DefaultItemReference parent) 
            : base(new ItemEntity() { Guid = newGuid, Type = type }, parent, true)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (!IsRoot && parent == null) throw new ArgumentNullException(nameof(parent));

            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        public DefaultItem(ItemEntity entity)
            : base(entity, new DefaultItemReference(entity.ParentGuid, entity.ParentType), false)
        {
            children.SetReadOnly(true);
            related.SetReadOnly(true);
        }

        internal void AddChildren(IEnumerable<ItemBase> refs)
        {
            children.SetReadOnly(false);
            children.AddRange(refs);
            children.SetReadOnly(true);
        }

        internal void AddRelated(IEnumerable<ItemBase> refs)
        {
            related.SetReadOnly(false);
            related.AddRange(refs);
            related.SetReadOnly(true);
        }
    }
}
