using System;
using Itemify.Core.Typing;

namespace Itemify.Core.Item
{
    public interface IItemReference : IComparable
    {
        Guid Guid { get; }
        TypeItem Type { get; }
    }
}