using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Itemify.Core;
using Itemify.Core.Item;
using Itemify.Logging;
using Itemify.Util;

namespace Itemify
{
    public class Itemify : IItemify
    {
        private readonly ItemProvider provider;
        private readonly ItemifySettings settings;
        private readonly ILogWriter log;

        public IItemReference Root => Item.Wrap(DefaultItem.Root);

        public Itemify(ItemifySettings settings, ILogWriter log)
        {
            this.settings = settings;
            this.log = log.NewRegion("Itemify");
            this.provider = new ItemProvider(settings.GetProviderSettings(), log);
        }


        public Guid Save(Item item)
        {
            this.log.Describe("Save Item", item);
            return provider.Save(item.GetInner());
        }

        public void SaveExisting(Item item)
        {
            this.log.Describe("Save Existing Item", item);
            provider.SaveExisting(item.GetInner());
        }

        public Guid SaveNew(Item item)
        {
            this.log.Describe("Save New Item", item);
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
            this.log.Describe("Set Relations", new
            {
                source,
                targets
            });
            provider.SetRelations(source.GetInner(), targets.Select(k => k.GetInner()));
        }

        public void AddRelations(IItemReference source, params IItemReference[] targets)
        {
            this.log.Describe("Add Relations", new
            {
                source,
                targets
            });
            provider.AddRelations(source.GetInner(), targets.Select(k => k.GetInner()));
        }

        public void RemoveRelations(IItemReference source, params string[] types)
        {
            this.log.Describe("Remove Relations", new
            {
                source,
                types
            });
            provider.RemoveRelations(source.GetInner(), types);
        }

        public Item GetItemByReference(Guid guid, string type)
        {
            return Item.Wrap(provider.GetItemByReference(new DefaultItemReference(guid, type)));
        }

        public Item GetItemByReference(Guid guid, string type, ItemResolving resolving)
        {
            return Item.Wrap(provider.GetItemByReference(new DefaultItemReference(guid, type), resolving));
        }

        public Item GetItemByReference(IItemReference r)
        {
            return Item.Wrap(provider.GetItemByReference(r.GetInner()));
        }

        public Item GetItemByReference(IItemReference r, ItemResolving resolving)
        {
            return Item.Wrap(provider.GetItemByReference(r.GetInner(), resolving));
        }

        /// <summary>
        /// Returns all items, which string value matchs a specific pattern. (case-insensitive)
        /// </summary>
        /// <param name="pattern">Wildcard: %</param>
        /// <param name="type"></param>
        /// <returns></returns>
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
            return provider.GetChildrenOfItemByReference(r.GetInner(), types).Select(Item.Wrap);
        }

        public void RemoveItemByReference(IItemReference r)
        {
            provider.RemoveItemByReference(r.GetInner());
        }

        public void ResetCompletely()
        {
            provider.Reset();
        }

        public IEnumerable<Item> GetItemsByTypes(params string[] types)
        {
            return GetItemsByTypes(ItemResolving.Default, types);
        }

        public IEnumerable<Item> GetItemsByTypes(ItemResolving resolving, params string[] types)
        {
            if (types.Length == 0)
                throw new ArgumentException("Please enter at least one type.", nameof(types));

            return provider.GetItemsByTypes(resolving, types)
                .Select(Item.Wrap);
        }
    }
}
