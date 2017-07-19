using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Exceptions;
using Itemify.Core.Item;
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

        public DefaultItemReference Root => DefaultItem.Root;


        public ItemProvider(ItemProviderSettings settings, ILogWriter log)
        {
            log = log.NewRegion(nameof(ItemProvider));
            var entityProviderLog = log.NewRegion(nameof(EntityProvider));
            var pool = PostgreSqlConnectionPoolFactory.GetPoolByConnectionString(settings.PostgreSqlConnectionString, settings.MaxConnections, settings.Timeout);
            var sqlProvider = new PostgreSqlProvider(pool, entityProviderLog.NewRegion("PostgreSQL"), settings.Schema);

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
            var relations = new KeyValuePair<Guid, string>(item.Guid, actualItem.Type);

            provider.InsertEntityRelations(item.Parent.Type, item.Parent.Guid, new[] { relations }, CHILDREN_MAPPING_TABLE_NAME, false);

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

            var typeName = item.Type;
            var guid = provider.Insert(typeName, item.GetEntity());
            var relations = new KeyValuePair<Guid, string>(item.Guid, typeName);

            provider.InsertEntityRelations(item.Parent.Type, item.Parent.Guid, new[] {relations}, CHILDREN_MAPPING_TABLE_NAME, false);

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


        public DefaultItem GetItemByReference(DefaultItemReference r)
        {
            return GetItemByReference(r, ItemResolving.Default);
        }

        public DefaultItem GetItemByReference(DefaultItemReference r, ItemResolving resolving)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));
            if (r.Equals(Root)) throw new ArgumentException("Cannot get root item: " + r, "r");

            var entity = provider.QuerySingleEntity(r.Type, r.Guid);
            if (entity == null)
                return null;

            return resolveItem(entity, resolving);
        }

        public IEnumerable<DefaultItem> GetChildrenOfItemByReference(DefaultItemReference r, ItemResolving resolving, params string[] types)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (types == null) throw new ArgumentNullException(nameof(types));

            return getChildrenOfItem(r, types, resolving);
        }

        public IEnumerable<DefaultItem> GetRelationsOfItemByReference(DefaultItemReference parent, ItemResolving resolving, params string[] types)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (types == null) throw new ArgumentNullException(nameof(types));

            return getRelationsOfItem(parent, types, resolving);
        }

        public void AddRelations(DefaultItemReference itemA, IEnumerable<DefaultItemReference> relatedItems)
        {
            var relations = relatedItems
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type));

            provider.InsertEntityRelations(itemA.Type, itemA.Guid, relations, RELATIONS_MAPPING_TABLE_NAME, false);
        }

        public void SetRelations(DefaultItemReference itemA, IEnumerable<DefaultItemReference> relatedItems)
        {
            var relations = relatedItems
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type));

            provider.InsertEntityRelations(itemA.Type, itemA.Guid, relations, RELATIONS_MAPPING_TABLE_NAME, true);
        }

        public void RemoveRelations(DefaultItem itemA)
        {
            provider.InsertEntityRelations(itemA.Type, itemA.Guid, new KeyValuePair<Guid, string>[0], RELATIONS_MAPPING_TABLE_NAME, true);
        }
        
        public void RemoveRelations(DefaultItemReference itemA, params string[] types)
        {
            if (itemA == null) throw new ArgumentNullException(nameof(itemA));
            if (types == null) throw new ArgumentNullException(nameof(types));

            provider.RemoveEntityRelations(itemA.Type, itemA.Guid, RELATIONS_MAPPING_TABLE_NAME, types);
        }

        public IEnumerable<DefaultItem> GetItemsByName(string pattern, string type, ItemResolving resolving)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryEntitiesByName(type, pattern);

            return resolveItems(items, resolving);
        }

        public IEnumerable<DefaultItem> GetItemsByStringValue(string pattern, string type, ItemResolving resolving)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryEntitiesByStringValue(type, pattern);

            return resolveItems(items, resolving);
        }

        public IEnumerable<DefaultItem> GetItemsByNumberValue(double from, double to, string type, ItemResolving resolving)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryEntitiesByNumberValue(type, from, to);

            return resolveItems(items, resolving);
        }

        public IEnumerable<DefaultItem> GetItemsByDateTimeValue(DateTime from, DateTime to, string type, ItemResolving resolving)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var items = provider.QueryEntitiesByDateTimeValue(type, from, to);

            return resolveItems(items, resolving);
        }


        private void saveChildren(DefaultItem item)
        {
            var relations = item.Children
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type));

            provider.InsertEntityRelations(item.Type, item.Guid, relations, CHILDREN_MAPPING_TABLE_NAME, true);

            var children = item.Children.OfType<DefaultItem>();
            foreach (var child in children)
            {
                Save(child);
            }
        }

        private DefaultItem resolveItem(ItemEntity entity, ItemResolving resolving)
        {
            var item = new DefaultItem(entity);

            using (var slog = log.NewRegion("Resolve Item").StartStopwatch())
            {
                if (resolving.ChildrenTypes.Any())
                {
                    var children = getChildrenOfItem(item, resolving.ChildrenTypes, ItemResolving.Default);
                    item.AddChildren(children);
                    slog.Describe($"Resolved {item.Children.Count} child(ren) by types", resolving.ChildrenTypes);
                }

                if (resolving.RelationsTypes.Any())
                {
                    var relatedItems = getRelationsOfItem(item, resolving.RelationsTypes, ItemResolving.Default);
                    item.AddRelated(relatedItems);
                    slog.Describe($"Resolved {item.Related.Count} relation(s) by types", resolving.RelationsTypes);
                }

                if (entity.ParentGuid.Equals(Root.Guid))
                {
                    item.Parent = Root;
                }
                else if (resolving.ResolveParent)
                {
                    item.Parent = GetItemByReference(item.Parent);
                    slog.Describe($"Resolved parent", item.Parent);
                }
            }

            return item;
        }

        private IEnumerable<DefaultItem> resolveItems(IEnumerable<ItemEntity> entities, ItemResolving resolving)
        {
            // TODO: Work on performance
            return entities.Select(k => resolveItem(k, resolving));
        }

        private IEnumerable<DefaultItem> getChildrenOfItem(DefaultItemReference itemRef, IEnumerable<string> types, ItemResolving resolving)
        {
            if (itemRef == null) throw new ArgumentNullException(nameof(itemRef));

            var ttypes = types.Select(k => k.ToCamelCase()).ToArray();
            var count = 0;

            if (ttypes.Length == 0)
            {
                var children = provider.QueryEntityRelations(itemRef.Type, itemRef.Guid,
                        CHILDREN_MAPPING_TABLE_NAME, false)
                    .Select(child => resolveItem(child, resolving))
                    .OrderBy(k => k.Created)
                    .ToArray();

                foreach (var child in children)
                {
                    yield return child;
                }

                log.Describe($"Resolved {children.Length} child(ren) of type: ANY for parent {itemRef}.");
            }
            else
            {

                foreach (var type in ttypes)
                {
                    count++;
                    var typeName = type;
                    var children = provider.QueryEntityRelations(itemRef.Type, itemRef.Guid, typeName,
                            CHILDREN_MAPPING_TABLE_NAME, false)
                        .Select(child => resolveItem(child, resolving))
                        .OrderBy(k => k.Created)
                        .ToArray();

                    foreach (var child in children)
                    {
                        yield return child;
                    }

                    log.Describe($"Resolved {children.Length} child(ren) of type: '{type}' for parent {itemRef}.");
                }
            }
        }

        private IEnumerable<DefaultItem> getRelationsOfItem(DefaultItemReference itemRef, IEnumerable<string> types, ItemResolving resolving)
        {
            types = types.Select(k => k.ToCamelCase());

            foreach (var type in types)
            {
                var typeName = type;
                var relatedItems = provider.QueryEntityRelations(itemRef.Type, itemRef.Guid, typeName,
                        RELATIONS_MAPPING_TABLE_NAME, true)
                    .Select(k => resolveItem(k, resolving))
                    .OrderBy(k => k.Created)
                    .ToArray();

                foreach (var item in relatedItems)
                {
                    yield return item;
                }

                log.Describe($"Resolved {relatedItems.Length} related item(s) of type: {type}.");
            }
        }

        public void RemoveItemByReference(DefaultItemReference r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));

            provider.DeleteEntity(r.Type, r.Guid);

            var children = GetChildrenOfItemByReference(r, ItemResolving.Default);
            children.ForEach(k => provider.Update(k.Type, new ItemEntity() { Guid = k.Guid, ParentGuid = Root.Guid, ParentType = Root.Type}));

            var childrenUpdated = provider.ReplaceEntityRelationSources(CHILDREN_MAPPING_TABLE_NAME, r.Guid, r.Type, Root.Guid, Root.Type, true); // Set parent of children to root item
            var relationsRemoved = provider.RemoveEntityRelations(r.Type, r.Guid, RELATIONS_MAPPING_TABLE_NAME);

            log.Describe($"Deleted item '{r}'.", new
            {
                childrenUpdated,
                relationsRemoved
            });
        }

        public void Reset()
        {
            provider.Reset();
        }

        public IEnumerable<DefaultItem> GetItemsByTypes(ItemResolving resolving, IEnumerable<string> types)
        {
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));
            if (types == null) throw new ArgumentNullException(nameof(types));

            return provider.QueryEntitiesByTableNames(types)
                .Select(e => resolveItem(e, resolving));
        }
    }
}
