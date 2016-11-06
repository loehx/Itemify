using System;
using Itemify.Keywording;
using Itemify.Typing;

namespace Itemify.Item
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
            Keywords = new KeywordSet();
            Types = new TypeSet();
        }


        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }

        public virtual int Type { get; }
        public virtual string TypeName { get; }

        public virtual TypeSet Types { get; }
        public virtual KeywordSet Keywords { get; }

        public virtual IItemicItem Parent { get; }
        public virtual ItemCollection<IItemicItem> Children { get; }
        public virtual ItemCollection<IItemicItem> Related { get; }

        public virtual DateTime Timestamp { get; }
    }
}
