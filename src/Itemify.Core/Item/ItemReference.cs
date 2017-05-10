using System;
using Itemify.Core.Item;

namespace Itemify.Core.ItemAccess
{
    public class ItemReference : IItemReference
    {
        public Guid Guid { get; }
        public string Type { get; }

        public ItemReference(Guid guid, string type)
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

        public int CompareTo(object obj)
        {
            var item = obj as DefaultItem;
            if (item != null)
                return -1;

            var reference = obj as IItemReference;
            if (reference != null)
            {
                return string.Compare(reference.Type, Type, StringComparison.Ordinal);
            }

            return 0;
        }
    }
}