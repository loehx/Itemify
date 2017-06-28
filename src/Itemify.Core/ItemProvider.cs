﻿using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Exceptions;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Logging;
using Itemify.Shared.Utils;

namespace Itemify.Core
{
    public class ItemProvider
    {
        private readonly EntityProvider provider;
        private readonly ILogWriter log;

        public const string CHILDREN_MAPPING_TABLE_NAME = "childrenMapping";
        public const string RELATIONS_MAPPING_TABLE_NAME = "relationsMapping";

        public IItemReference Root => DefaultItem.Root;


        public ItemProvider(ItemProviderSettings settings, ILogWriter log)
        {
            var entityProviderLog = log.NewRegion(nameof(EntityProvider));
            var pool = new PostgreSqlConnectionPool(settings.PostgreSqlConnectionString, settings.MaxConnections, settings.Timeout);
            var sqlProvider = new PostgreSqlProvider(pool, entityProviderLog.NewRegion("PostgreSQL"), settings.Schema);
            sqlProvider.EnsureSchemaExists();

            this.provider = new EntityProvider(sqlProvider, entityProviderLog);
            this.log = log;
        }


        // TODO: Make internal
        public ItemProvider(EntityProvider provider, ILogWriter log)
        {
            this.provider = provider;
            this.log = log;
        }

        public Guid Save(DefaultItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var actualItem = item as DefaultItem;
            if (actualItem == null)
                throw new ArgumentException($"Unknown item type: '{item.GetType().Name}'", nameof(item));

            var guid = provider.Upsert(actualItem.Type, actualItem.GetEntity());

            // Should not save children implicitly
            // if (item.Children.Count > 0)
            //    saveChildren(item);

            return guid;
        }

        public void SaveExisting(DefaultItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var actualItem = item as DefaultItem;
            if (actualItem == null)
                throw new ArgumentException($"Unknown item type: '{item.GetType().Name}'", nameof(item));

            try
            {
                provider.Update(actualItem.Type, actualItem.GetEntity());

                // Should not save children implicitly
                // if (item.Children.Count > 0)
                //    saveChildren(item);
            }
            catch (EntitityNotFoundException e)
            {
                throw new ItemNotFoundException(item, e);
            }
        }

        public Guid SaveNew(DefaultItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var guid = provider.Insert(item.Type, item.GetEntity());
            var relations = new KeyValuePair<Guid, string>(item.Guid, parseTypeName(item.Type));

            provider.InsertItemRelations(parseTypeName(item.Parent.Type), item.Parent.Guid, new[] {relations}, CHILDREN_MAPPING_TABLE_NAME, false);

            return guid;
        }

        internal void SaveNew(IEnumerable<DefaultItem> items)
        {
            var iitems = items as IReadOnlyCollection<DefaultItem> ?? items.ToArray();
            if (iitems.Count == 0)
                throw new ArgumentException("No items passed to be saved.", nameof(items));

            var type = iitems.First().Type;
            if (!iitems.All(k => k.Type.Equals(type)))
                throw new ArgumentException("Items mixed up. The passed items can only of one specific type.", nameof(items));

            provider.Insert(type, iitems.Select(k => k.GetEntity()));
        }


        public DefaultItem GetItemByReference(IItemReference r)
        {
            return GetItemByReference(r, ItemResolving.Default);
        }

        public DefaultItem GetItemByReference(IItemReference r, ItemResolving resolving)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));
            if (r.Equals(Root)) throw new ArgumentException("Cannot get root item: " + r, "r");

            var entity = provider.QuerySingleItem(parseTypeName(r.Type), r.Guid);
            if (entity == null)
                return null;

            return resolveItem(entity, resolving);
        }

        public IEnumerable<DefaultItem> GetChildrenOfItemByReference(IItemReference r, params string[] types)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (types == null) throw new ArgumentNullException(nameof(types));

            return getChildrenOfItem(r, types);
        }

        public IEnumerable<DefaultItem> GetRelationsOfItemByReference(IItemReference parent, params string[] types)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (types == null) throw new ArgumentNullException(nameof(types));

            return getRelationsOfItem(parent, types);
        }

        public void AddRelations(IItemReference itemA, params IItemReference[] relatedItems)
        {
            var relations = relatedItems
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type));

            provider.InsertItemRelations(itemA.Type, itemA.Guid, relations, RELATIONS_MAPPING_TABLE_NAME, false);
        }

        public void SetRelations(IItemReference itemA, params IItemReference[] relatedItems)
        {
            var relations = relatedItems
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type));

            provider.InsertItemRelations(itemA.Type, itemA.Guid, relations, RELATIONS_MAPPING_TABLE_NAME, true);
        }

        public void RemoveRelations(DefaultItem itemA)
        {
            provider.InsertItemRelations(itemA.Type, itemA.Guid, new KeyValuePair<Guid, string>[0], RELATIONS_MAPPING_TABLE_NAME, true);
        }
        
        public void RemoveRelations(IItemReference itemA, params string[] types)
        {
            if (itemA == null) throw new ArgumentNullException(nameof(itemA));
            if (types == null) throw new ArgumentNullException(nameof(types));

            provider.DeleteItemRelations(itemA.Type, itemA.Guid, RELATIONS_MAPPING_TABLE_NAME, types);
        }

        public IEnumerable<DefaultItem> GetItemsByStringValue(string pattern, string type, ItemResolving resolving)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryItemsByStringValue(type, pattern);

            return resolveItems(items, resolving);
        }

        public IEnumerable<DefaultItem> GetItemsByNumberValue(double from, double to, string type, ItemResolving resolving)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryItemsByNumberValue(type, from, to);

            return resolveItems(items, resolving);
        }

        public IEnumerable<DefaultItem> GetItemsByDateTimeValue(DateTime from, DateTime to, string type, ItemResolving resolving)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryItemsByDateTimeValue(type, from, to);

            return resolveItems(items, resolving);
        }


        private void saveChildren(DefaultItem item)
        {
            var relations = item.Children
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type));

            provider.InsertItemRelations(item.Type, item.Guid, relations, CHILDREN_MAPPING_TABLE_NAME, true);

            var children = item.Children.OfType<DefaultItem>();
            foreach (var child in children)
            {
                Save(child);
            }
        }

        private DefaultItem resolveItem(ItemEntity entity, ItemResolving resolving)
        {
            var item = new DefaultItem(entity);

            if (resolving.ChildrenTypes.Any())
            {
                var children = getChildrenOfItem(item, resolving.ChildrenTypes);
                item.AddChildren(children);
            }

            if (resolving.RelationsTypes.Any())
            {
                var relatedItems = getRelationsOfItem(item, resolving.RelationsTypes);
                item.AddRelated(relatedItems);
            }

            if (resolving.ResolveParent)
            {
                item.Parent = GetItemByReference(item.Parent);
            }

            return item;
        }

        private IEnumerable<DefaultItem> resolveItems(IEnumerable<ItemEntity> entities, ItemResolving resolving)
        {
            // TODO: Work on performance
            return entities.Select(k => resolveItem(k, resolving));
        }

        private IEnumerable<DefaultItem> getChildrenOfItem(IItemReference itemRef, IEnumerable<string> types)
        {

            foreach (var type in types)
            {
                var typeName = parseTypeName(type);
                var children = provider.QueryItemsByRelation(itemRef.Type, itemRef.Guid, typeName,
                        CHILDREN_MAPPING_TABLE_NAME, false)
                    .Select(child => new DefaultItem(child))
                    .ToArray();

                foreach (var child in children)
                {
                    yield return child;
                }

                log.Describe($"Resolved {children.Length} children of type: {type}.");
            }
        }

        private IEnumerable<DefaultItem> getRelationsOfItem(IItemReference itemRef, IEnumerable<string> types)
        {

            foreach (var type in types)
            {
                var typeName = parseTypeName(type);
                var relatedItems = provider.QueryItemsByRelation(itemRef.Type, itemRef.Guid, typeName,
                        RELATIONS_MAPPING_TABLE_NAME, true)
                    .Select(child => new DefaultItem(child))
                    .ToArray();

                foreach (var item in relatedItems)
                {
                    yield return item;
                }

                log.Describe($"Resolved {relatedItems.Length} related item(s) of type: {type}.");
            }
        }


        private static string parseTypeName(string name)
        {
            return name.ToCamelCase();
        }

        public void RemoveItemByReference(IItemReference r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));

            provider.Delete(r.Type, r.Guid);

            log.Describe($"Deleted item '{r}'.");
        }
    }
}
