using System;
using Itemify.Core.Item;

namespace Itemify.Core.Spec.Example_A
{
    internal class Device
    {
        private readonly global::Itemify.Item item;

        public Guid Guid => item.Guid;

        public string Name
        {
            get { return item.Name; }
            set { item.Name = value; }
        }

        public Device(global::Itemify.Item item)
        {
            this.item = item;
        }
    }
}
