using System;

namespace Itemify
{
    public interface IItemReference : IComparable
    {
        Guid Guid { get; }
        string Type { get; }
    }
}