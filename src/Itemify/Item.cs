using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Itemify.Core.Item;
using Itemify.Shared.Utils;

namespace Itemify
{
    public class Item
    {
        private DefaultItem inner;

        public Item()
        {
            inner = new DefaultItem();
        }

        public Item(string type)
        {
            inner = new DefaultItem(type);
        }

        internal Item(DefaultItem inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));

            this.inner = inner;
        }


        public int Revision
        {
            get { return inner.Revision;  }
        }

        public bool Debug
        {
            get { return inner.Debug; }
        }

        public bool HasBody
        {
            get { return inner.HasBody; }
        }

        public bool IsParentResolved
        {
            get { return inner.IsParentResolved; }
        }

        public IItemReference Parent
        {
            get { return inner.Parent; }
            set { inner.Parent = value; }
        }

        public DateTime Created
        {
            get { return inner.Created; }
        }

        public DateTime Modified
        {
            get { return inner.Modified; }
        }

        public bool IsNew
        {
            get { return inner.IsNew; }
        }

        public Guid Guid
        {
            get { return inner.Guid; }
            set { inner.Guid = value; }
        }

        public string Type
        {
            get { return inner.Type; }
            set { inner.Type = value; }
        }

        public string Name
        {
            get { return inner.Name; }
            set { inner.Name = value; }
        }

        public double? ValueNumber
        {
            get { return inner.ValueNumber; }
            set { inner.ValueNumber = value; }
        }

        public DateTime? ValueDate
        {
            get { return inner.ValueDate; }
            set { inner.ValueDate = value; }
        }

        public string ValueString
        {
            get { return inner.ValueString; }
            set { inner.ValueString = value; }
        }

        public int Order
        {
            get { return inner.Order; }
            set { inner.Order = value; }
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

        public IReadOnlyCollection<Item> Children
        {
            get { return inner.Children.Cast<DefaultItem>().SelectList(Wrap); }
        }

        public IReadOnlyCollection<Item> Related
        {
            get { return inner.Related.Cast<DefaultItem>().SelectList(Wrap); }
        }

        internal DefaultItem GetInner() => this.inner;

        internal static Item Wrap(DefaultItem item)
        {
            return item == null ? null : new Item(item);
        }
    }
}
