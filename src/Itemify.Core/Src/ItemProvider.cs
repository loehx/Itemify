using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Exceptions;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Core.Typing;
using Itemify.Shared.Logging;

namespace Itemify.Core
{
    public class ItemProvider
    {
        private readonly EntityProvider provider;
        private readonly ILogWriter log;
        private readonly ItemContext context;

        public const string CHILDREN_MAPPING_TABLE_NAME = "ChildrenMapping";

        public IItemReference Root => new ItemReference(Guid.Empty, context.TypeManager.GetTypeItem(DefaultTypes.Root));

        internal ItemProvider(EntityProvider provider, TypeManager typeManager, ILogWriter log)
        {
            this.provider = provider;
            this.log = log;
            this.context = new ItemContext(typeManager);
        }

        public IItem NewItem(IItemReference parent, TypeItem type)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (type == null) throw new ArgumentNullException(nameof(type));

            return new DefaultItem(Guid.NewGuid(), type, context, parent);
        }

        public Guid Save(IItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var actualItem = item as DefaultItem;
            if (actualItem == null)
                throw new ArgumentException($"Unknown item type: '{item.GetType().Name}'", nameof(item));

            var guid = provider.Upsert(actualItem.Type.Name, actualItem.GetEntity());

            if (item.Children.Count > 0)
                saveChildren(item);

            return guid;
        }

        public void SaveExisting(IItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var actualItem = item as DefaultItem;
            if (actualItem == null)
                throw new ArgumentException($"Unknown item type: '{item.GetType().Name}'", nameof(item));

            try
            {
                provider.Update(actualItem.Type.Name, actualItem.GetEntity());

                if (item.Children.Count > 0)
                    saveChildren(item);
            }
            catch (EntitityNotFoundException e)
            {
                throw new ItemNotFoundException(item, e);
            }
        }

        public Guid SaveNew(IItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var actualItem = item as DefaultItem;
            if (actualItem == null)
                throw new ArgumentException($"Unknown item type: '{item.GetType().Name}'", nameof(item));

            var guid = provider.Insert(actualItem.Type.Name, actualItem.GetEntity());

            if (item.Children.Count > 0)
                saveChildren(item);

            return guid;
        }


        public IItem GetItemByReference(IItemReference r)
        {
            return GetItemByReference(r, ItemResolving.Default);
        }

        public IItem GetItemByReference(IItemReference r, ItemResolving resolving)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));

            var entity = provider.QuerySingleItem(r.Type.Name, r.Guid);
            if (entity == null)
                return null;

            return resolveItem(entity, resolving);
        }

        private void saveChildren(IItem child)
        {
            var relations = child.Children
                .Select(k => new KeyValuePair<Guid, string>(k.Guid, k.Type.Name));

            provider.InsertItemRelations(child.Type.Name, child.Guid, relations, CHILDREN_MAPPING_TABLE_NAME, true);

            var items = child.Children.OfType<IItem>();
            foreach (var item in items)
            {
                Save(item);
            }
        }

        private IItem resolveItem(ItemEntity entity, ItemResolving resolving)
        {
            var item = new DefaultItem(entity, context);

            if (resolving.ChildrenTypes.Any())
            {
                var types = resolving.ChildrenTypes
                    .Select(context.TypeManager.GetTypeItem)
                    .ToArray();

                foreach (var type in types)
                {
                    var children = provider.QueryItemsByRelation(item.Type.Name, entity.Guid, type.Name,
                            CHILDREN_MAPPING_TABLE_NAME)
                            .Select(child => new DefaultItem(child, context))
                            .ToArray();

                    item.Children.AddRange(children);
                    log.Describe($"Resolved {children.Length} children of type: {type}.");
                }
            }

            return item;
        }
    }

    public class ItemResolving
    {
        private List<Enum> children;
        private List<Enum> relations;

        internal ItemResolving()
        {
        }

        public bool Empty => children == null && relations == null;
        public IEnumerable<Enum> ChildrenTypes => children ?? Enumerable.Empty<Enum>();
        public IEnumerable<Enum> RelationsTypes => relations ?? Enumerable.Empty<Enum>();


        public ItemResolving ChildrenOfType(params Enum[] types)
        {
            if (children == null)
                children = new List<Enum>(types.Length);

            children.AddRange(types);

            return this;
        }

        public ItemResolving RelatedItemsOfType(params Enum[] types)
        {
            if (relations == null)
                relations = new List<Enum>(types.Length);

            relations.AddRange(types);

            return this;
        }


        public static ItemResolving Default => new ItemResolving();
    }
}
