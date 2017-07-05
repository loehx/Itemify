using System;

namespace Itemify.Core.Item
{
    public class DefaultItemReference : IComparable
    {
        public Guid Guid { get; }
        public string Type { get; }

        public DefaultItemReference(Guid guid, string type)
        {
            this.Guid = guid;
            this.Type = type;
        }

        protected bool Equals(DefaultItemReference other)
        {
            return Guid.Equals(other.Guid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is DefaultItemReference)) return false;
            return Equals((DefaultItemReference)obj);
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

            var reference = obj as DefaultItemReference;
            if (reference != null)
            {
                if (Type.Equals(reference.Type, StringComparison.OrdinalIgnoreCase))
                    return reference.Guid.CompareTo(Guid);

                return string.Compare(reference.Type, Type, StringComparison.Ordinal);
            }

            return 0;
        }

        public override string ToString()
        {
            return $"<{Type}> {Guid}";
        }
    }
}