using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Itemify.Core;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.Typing;
using Itemify.Logging;

namespace Itemify
{
    public class Itemify
    {
        private readonly ItemProvider provider;
        private readonly ItemifySettings settings;
        private readonly TypeManager typeManager;


        public Itemify(ItemifySettings settings, ILogWriter log)
        {
            this.settings = settings;
            this.typeManager = settings.GetTypeManager();
            this.provider = new ItemProvider(settings.GetProviderSettings(), this.typeManager, log);
        }

        internal IItem NewItem(Enum type)
        {
            return NewItem(provider.Root, type);
        }

        internal IItem NewItem(IItemReference parent, Enum type)
        {
            var typeItem = typeManager.GetTypeItem(type);
            return provider.NewItem(parent, typeItem);
        }



        public Guid Save(IItem item)
        {
            return provider.Save(item);
        }

        public void SaveExisting(IItem item)
        {
            provider.SaveExisting(item);
        }

        public Guid SaveNew(IItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            try
            {
                return provider.SaveNew(item);
            }
            catch (Exception err)
            {
                throw new Exception($"Item »{ item.Name }« could not be saved.", err);   
            }
        }

        public IItem GetItemByReference(Guid guid, Enum type)
        {
            return provider.GetItemByReference(new ItemReference(guid, typeManager.GetTypeItem(type)));
        }

        public IItem GetItemByReference(Guid guid, Enum type, ItemResolving resolving)
        {
            return provider.GetItemByReference(new ItemReference(guid, typeManager.GetTypeItem(type)));
        }

        public IItem GetItemByReference(IItemReference r)
        {
            return provider.GetItemByReference(r);
        }

        public IItem GetItemByReference(IItemReference r, ItemResolving resolving)
        {
            return provider.GetItemByReference(r, resolving);
        }

        public IEnumerable<IItem> GetChildrenOfItemByReference(IItemReference r, params Enum[] types)
        {
            return provider.GetChildrenOfItemByReference(r, types);
        }
    }
}
