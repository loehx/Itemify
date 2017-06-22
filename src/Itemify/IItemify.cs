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
        IEnumerable<Item> GetChildrenOfItemByReference(IItemReference r, params string[] types);
        Guid Save(Item item);
        void SaveExisting(Item item);
        void SetRelations(IItemReference source, params IItemReference[] targets);
        void AddRelations(IItemReference source, params IItemReference[] targets);
        void RemoveRelations(IItemReference source, params string[] types);

        /// <summary>
        /// Returns all items, which string value matchs a specific pattern. (case-insensitive)
        /// </summary>
        /// <param name="pattern">Wildcard: %</param>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<Item> GetItemsByStringValue(string pattern, string type);

        /// <summary>
        /// Returns all items, which string value matchs a specific pattern. (case-insensitive)
        /// </summary>
        /// <param name="pattern">Wildcard: %</param>
        /// <param name="type"></param>
        /// <param name="resolving"></param>
        /// <returns></returns>
        IEnumerable<Item> GetItemsByStringValue(string pattern, string type, ItemResolving resolving);

        /// <summary>
        /// Returns all items, which number value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<Item> GetItemsByNumberValue(double from, double to, string type);

        /// <summary>
        /// Returns all items, which number value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <param name="resolving"></param>
        /// <returns></returns>
        IEnumerable<Item> GetItemsByNumberValue(double from, double to, string type, ItemResolving resolving);

        /// <summary>
        /// Returns all items, which date value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<Item> GetItemsByDateTimeValue(DateTime from, DateTime to, string type);

        /// <summary>
        /// Returns all items, which date value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<Item> GetItemsByDateTimeValue(DateTime from, DateTime to, string type, ItemResolving resolving);
    }
}