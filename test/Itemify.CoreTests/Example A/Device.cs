using System;
using Itemify.Core.Item;

namespace Itemify.Core.Spec.Example_A
{
    internal class Device
    {
        private readonly IItem item;

        public Guid Guid => item.Guid;

        public string Name
        {
            get { return item.Name; }
            set { item.Name = value; }
        }

        public Device(IItem item)
        {
            this.item = item;
        }
    }
}
