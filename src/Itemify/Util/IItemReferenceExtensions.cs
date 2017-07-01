using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Item;

namespace Itemify.Util
{
    public static class ItemReferenceExtensions
    {
        public static Item AsItem(this IItemReference reference)
        {
            return reference as Item;
        }

        internal static DefaultItemReference GetInner(this IItemReference @ref)
        {
            if (@ref == null) return null;

            if (@ref is Item) return ((Item) @ref).GetInner();
            if (@ref is ItemReference) return ((ItemReference)@ref).GetInner();

            throw new ArgumentException($"Unknown type: {@ref.GetType().Name}", nameof(@ref));
        }

        public static IEnumerable<Item> AsItem(this IEnumerable<IItemReference> references)
        {
            return references.OfType<Item>();
        }
    }
}
