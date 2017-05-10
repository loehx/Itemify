using System;

namespace Itemify.Core.Item
{
    public interface IItemReference : IComparable
    {
        Guid Guid { get; }
        string Type { get; }
    }
}