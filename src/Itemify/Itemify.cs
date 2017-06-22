using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Itemify.Core;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Logging;

namespace Itemify
{
    public class Itemify : IItemify
    {
        private readonly ItemProvider provider;
        private readonly ItemifySettings settings;


        public Itemify(ItemifySettings settings, ILogWriter log)
        {
            this.settings = settings;
            this.provider = new ItemProvider(settings.GetProviderSettings(), log);
        }


        public Guid Save(Item item)
        {
            return provider.Save(item.GetInner());
        }

        public void SaveExisting(Item item)
        {
            provider.SaveExisting(item.GetInner());
        }

        public Guid SaveNew(Item item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                return provider.SaveNew(item.GetInner());
            }
            catch (Exception err)
            {
                throw new Exception($"Item »{ item.Name }« could not be saved.", err);   
            }
        }

        public void SetRelations(IItemReference source, params IItemReference[] targets)
        {
            provider.SetRelations(source, targets);
        }

        public void AddRelations(IItemReference source, params IItemReference[] targets)
        {
            provider.AddRelations(source, targets);
        }

        public void RemoveRelations(IItemReference source, params string[] types)
        {
            provider.RemoveRelations(source, types);
        }

        public Item GetItemByReference(Guid guid, string type)
        {
            return Item.Wrap(provider.GetItemByReference(new ItemReference(guid, type)));
        }

        public Item GetItemByReference(Guid guid, string type, ItemResolving resolving)
        {
            return Item.Wrap(provider.GetItemByReference(new ItemReference(guid, type), resolving));
        }

        public Item GetItemByReference(IItemReference r)
        {
            return Item.Wrap(provider.GetItemByReference(r));
        }

        public Item GetItemByReference(IItemReference r, ItemResolving resolving)
        {
            return Item.Wrap(provider.GetItemByReference(r, resolving));
        }

        public IEnumerable<Item> GetItemsByStringValue(string pattern, string type)
        {
            return GetItemsByStringValue(pattern, type, ItemResolving.Default);
        }

        /// <summary>
        /// Returns all items, which string value matchs a specific pattern. (case-insensitive)
        /// </summary>
        /// <param name="pattern">Wildcard: %</param>
        /// <param name="type"></param>
        /// <param name="resolving"></param>
        /// <returns></returns>
        public IEnumerable<Item> GetItemsByStringValue(string pattern, string type, ItemResolving resolving)
        {
            return provider.GetItemsByStringValue(pattern, type, resolving).Select(Item.Wrap);
        }

        /// <summary>
        /// Returns all items, which number value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Item> GetItemsByNumberValue(double from, double to, string type)
        {
            return GetItemsByNumberValue(from, to, type, ItemResolving.Default);
        }

        /// <summary>
        /// Returns all items, which number value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <param name="resolving"></param>
        /// <returns></returns>
        public IEnumerable<Item> GetItemsByNumberValue(double from, double to, string type, ItemResolving resolving)
        {
            return provider.GetItemsByNumberValue(from, to, type, resolving).Select(Item.Wrap);
        }

        /// <summary>
        /// Returns all items, which date value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Item> GetItemsByDateTimeValue(DateTime from, DateTime to, string type)
        {
            return GetItemsByDateTimeValue(from, to, type, ItemResolving.Default);
        }

        /// <summary>
        /// Returns all items, which date value is within a specific range. (FROM &lt;= VALUE &lt;= TO)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Item> GetItemsByDateTimeValue(DateTime from, DateTime to, string type, ItemResolving resolving)
        {
            return provider.GetItemsByDateTimeValue(from, to, type, resolving).Select(Item.Wrap);
        }

        public IEnumerable<Item> GetChildrenOfItemByReference(IItemReference r, params string[] types)
        {
            return provider.GetChildrenOfItemByReference(r, types).Select(Item.Wrap);
        }
    }
}
