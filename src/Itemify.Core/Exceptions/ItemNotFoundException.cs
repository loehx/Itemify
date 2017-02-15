using System;
using Itemify.Core.Item;

namespace Itemify.Core
{
    public class ItemNotFoundException : Exception
    {
        public ItemNotFoundException(IItemReference reference, Exception inner)
            : base($"Item with ID {reference.Guid} of type {reference.Type} could not be found.")
        {
        }
    }
}