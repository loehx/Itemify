using System;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql;
using Itemify.Core.Typing;

namespace Itemify.Core
{
    public class ItemProvider
    {
        private readonly EntityProvider provider;
        private readonly ItemContext context;

        public IItemReference Root => new ItemReference(Guid.Empty, context.TypeManager.GetTypeItem(DefaultTypes.Root));

        internal ItemProvider(EntityProvider provider, TypeManager typeManager)
        {
            this.provider = provider;
            this.context = new ItemContext(typeManager);
        }

        public IItem NewItem(IItemReference parent, TypeItem type)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            return new DefaultItem(Guid.NewGuid(), type, context, parent);
        }

        public Guid Save(IItem item, bool createIfNotExists)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var actualItem = item as DefaultItem;
            if (actualItem == null)
                throw new ArgumentException($"Unknown item type: '{item.GetType().Name}'", nameof(item));

            return provider.Upsert(item.Type.Name, actualItem.GetEntity());
        }
    }

    internal class ItemContext
    {
        public ItemContext(TypeManager typeManager)
        {
            TypeManager = typeManager;
        }

        public TypeManager TypeManager { get; }
    }
}
