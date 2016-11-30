using System;
using Itemify.Core.Keywording;
using Itemify.Core.Typing;

namespace Itemify.Core.Item
{
    public class ItemBase : IItemicItem
    {
        internal ItemBase()
        {
        }

        protected ItemBase(IItemicItem parent)
        {
            Name = "AnoymousItem";
            Children = new ItemCollection<IItemicItem>();
            Related = new ItemCollection<IItemicItem>();
            Parent = parent;
            SubTypes = new TypeSet();
        }


        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }

        public virtual string TypeName { get; }

        public virtual TypeSet SubTypes { get; }

        public virtual IItemicItem Parent { get; }
        public virtual ItemCollection<IItemicItem> Children { get; }
        public virtual ItemCollection<IItemicItem> Related { get; }

        public virtual DateTime Timestamp { get; }
    }
}
