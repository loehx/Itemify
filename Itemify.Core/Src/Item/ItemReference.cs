using System;
using Itemify.Core.Item;
using Itemify.Core.Typing;

namespace Itemify.Core.ItemAccess
{
    public class ItemReference : IItemReference
    {
        public Guid Guid { get; }
        public TypeItem Type { get; }

        public ItemReference(Guid guid, TypeItem type)
        {
            this.Guid = guid;
            this.Type = type;
        }

        protected bool Equals(IItemReference other)
        {
            return Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is IItemReference)) return false;
            return Equals((IItemReference)obj);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

    }
}