using System;
using System.Collections.Generic;
using System.Text;
using Itemify.Core.Item;

namespace Itemify
{
    public class ItemReference : IItemReference
    {
        private readonly Core.Item.DefaultItemReference inner;

        internal ItemReference(Core.Item.DefaultItemReference inner)
        {
            this.inner = inner;

            if (inner is ItemBase)
                throw new ArgumentException($"Cannot wrap {nameof(ItemBase)} in {nameof(ItemReference)}.", nameof(inner));
        }

        public ItemReference(Guid guid, string type)
        {
            this.inner = new Core.Item.DefaultItemReference(guid, type);
        }

        public Guid Guid => inner.Guid;
        public string Type => inner.Type;
        public bool IsItem => false;

        internal Core.Item.DefaultItemReference GetInner() => inner;

        public int CompareTo(object obj)
        {
            throw new NotImplementedException(); // TODO: Remove this
        }
    }
}
