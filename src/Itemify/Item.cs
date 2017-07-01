using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Itemify.Core.Item;
using Itemify.Shared.Utils;
using Itemify.Util;

namespace Itemify
{
    public class Item : IItemReference
    {
        private DefaultItem inner;

        public Item()
        {
            inner = new DefaultItem();
        }

        public Item(string type)
        {
            inner = new DefaultItem(Guid.Empty, type);
        }

        public Item(Guid guid, string type)
        {
            inner = new DefaultItem(guid, type);
        }

        internal Item(DefaultItem inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            this.inner = inner;
        }


        public int Revision => inner.Revision;
        public bool Debug => inner.Debug;
        public bool HasBody => inner.HasBody;
        public bool IsParentResolved => inner.IsParentResolved;

        public IItemReference Parent
        {
            get => Item.Wrap(inner.Parent);
            set => inner.Parent = value.GetInner();
        }

        public DateTime Created => inner.Created;
        public DateTime Modified => inner.Modified;
        public bool IsNew => inner.IsNew;

        public Guid Guid
        {
            get => inner.Guid;
            set => inner.Guid = value;
        }

        public string Type
        {
            get => inner.Type;
            set => inner.Type = value;
        }

        public bool IsItem => true;

        public string Name
        {
            get => inner.Name;
            set => inner.Name = value;
        }

        public double? ValueNumber
        {
            get => inner.ValueNumber;
            set => inner.ValueNumber = value;
        }

        public DateTime? ValueDate
        {
            get => inner.ValueDate;
            set => inner.ValueDate = value;
        }

        public string ValueString
        {
            get => inner.ValueString;
            set => inner.ValueString = value;
        }

        public int Order
        {
            get => inner.Order;
            set => inner.Order = value;
        }

        public T GetBody<T>()
        {
            return inner.GetBody<T>();
        }

        public T TryGetBody<T>()
        {
            return inner.TryGetBody<T>();
        }

        public void SetBody(object body)
        {
            inner.SetBody(body);
        }

        public void SetBody(object body, bool beatify)
        {
            inner.SetBody(body, beatify);
        }

        public bool Equals(ItemBase item)
        {
            return inner.Equals(item);
        }

        public int CompareTo(object obj)
        {
            return inner.CompareTo(obj);
        }

        public IReadOnlyCollection<Item> Children => inner.Children.Cast<DefaultItem>().SelectList(Wrap);

        public IReadOnlyCollection<Item> Related => inner.Related.Cast<DefaultItem>().SelectList(Wrap);

        internal DefaultItem GetInner() => this.inner;

        internal static Item Wrap(DefaultItem item)
        {
            return item == null ? null : new Item(item);
        }

        internal static IItemReference Wrap(Core.Item.DefaultItemReference itemRef)
        {
            if (itemRef == null) return null;

            var @ref = itemRef as DefaultItem;
            return @ref != null ? (IItemReference) new Item(@ref) : new ItemReference(itemRef);
        }

        public override string ToString()
        {
            return $"{Name} <{Type}> ({(IsNew ? "NEW" : "REV:" + Revision)})";
        }
    }
}
