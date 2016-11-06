using System;
using Itemify.Keywording;
using Itemify.Typing;

namespace Itemify.Item
{
    public interface IItemicItem
    {
        ItemCollection<IItemicItem> Children { get; }
        Guid Id { get; set; }
        KeywordSet Keywords { get; }
        string Name { get; set; }
        IItemicItem Parent { get; }
        ItemCollection<IItemicItem> Related { get; }
        DateTime Timestamp { get; }
        TypeSet Types { get; }
    }
}