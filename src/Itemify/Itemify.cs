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

        public IEnumerable<Item> GetChildrenOfItemByReference(IItemReference r, params string[] types)
        {
            return provider.GetChildrenOfItemByReference(r, types).Select(Item.Wrap);
        }
    }
}
