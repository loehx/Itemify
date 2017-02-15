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

            var relations = new KeyValuePair<Guid, string>(item.Parent.Guid, item.Parent.Type.Name);

            provider.InsertItemRelations(item.Parent.Type.Name, item.Parent.Guid, new[] {relations}, CHILDREN_MAPPING_TABLE_NAME, true);

            return guid;
        }

        internal void SaveNew(IEnumerable<IItem> items)
        {
            var iitems = items as IReadOnlyCollection<IItem> ?? items.ToArray();
            if (iitems.Count == 0)
                throw new ArgumentException("No items passed to be saved.", nameof(items));

            var type = iitems.First().Type;
            if (!iitems.All(k => k.Type.Equals(type)))
                throw new ArgumentException("Items mixed up. The passed items can only of one specific type.", nameof(items));

            if (!iitems.All(k => k is DefaultItem))
                throw new ArgumentException($"Items must be of type {nameof(DefaultItem)}.", nameof(items));

            provider.Insert(type.ToStringValue(), iitems.Cast<DefaultItem>().Select(k => k.GetEntity()));
        }


        public IItem GetItemByReference(IItemReference r)
        {
            return GetItemByReference(r, ItemResolving.Default);
        }

        public IItem GetItemByReference(IItemReference r, ItemResolving resolving)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (resolving == null) throw new ArgumentNullException(nameof(resolving));
            if (r.Equals(Root)) throw new ArgumentException("Cannot get root item: " + r, "r");

            var entity = provider.QuerySingleItem(r.Type.Name, r.Guid);
            if (entity == null)
                return null;

            return resolveItem(entity, resolving);
        }

        public IEnumerable<IItem> GetChildrenOfItemByReference(IItemReference r, params Enum[] types)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (types == null) throw new ArgumentNullException(nameof(types));

            return getChildrenOfItem(r, types);
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
                var children = getChildrenOfItem(item, resolving.ChildrenTypes);
                item.AddChildren(children);
            }

            return item;
        }

        private IEnumerable<DefaultItem> getChildrenOfItem(IItemReference itemRef, IEnumerable<Enum> types)
        {
            var ttypes = types
                .Select(context.TypeManager.GetTypeItem)
                .ToArray();

            foreach (var type in ttypes)
            {
                var children = provider.QueryItemsByRelation(itemRef.Type.Name, itemRef.Guid, type.Name,
                        CHILDREN_MAPPING_TABLE_NAME)
                    .Select(child => new DefaultItem(child, context))
                    .ToArray();

                foreach (var child in children)
                {
                    yield return child;
                }

                log.Describe($"Resolved {children.Length} children of type: {type}.");
            }
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
