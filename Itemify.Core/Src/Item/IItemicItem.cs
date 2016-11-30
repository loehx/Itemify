using System;
using Itemify.Core.Keywording;
using Itemify.Core.Typing;

namespace Itemify.Core.Item
{
    public interface IItemicItem
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string TypeName { get; }
        TypeSet SubTypes { get; }
        IItemicItem Parent { get; }
        ItemCollection<IItemicItem> Children { get; }
        ItemCollection<IItemicItem> Related { get; }
        DateTime Timestamp { get; }
    }
}