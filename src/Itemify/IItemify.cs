using System;
using System.Collections.Generic;
using Itemify.Core;
using Itemify.Core.Item;

namespace Itemify
{
    public interface IItemify
    {
        Item GetItemByReference(Guid guid, string type);
        Item GetItemByReference(Guid guid, string type, ItemResolving resolving);
        Item GetItemByReference(IItemReference r);
        Item GetItemByReference(IItemReference r, ItemResolving resolving);
        IEnumerable<Item> GetItemsByStringValue(string value, string type);
        IEnumerable<Item> GetItemsByStringValue(string value, string type, ItemResolving resolving);
        IEnumerable<Item> GetChildrenOfItemByReference(IItemReference r, params string[] types);
        Guid Save(Item item);
        void SaveExisting(Item item);
        void AddRelation(IItemReference source, IItemReference target);
    }
}