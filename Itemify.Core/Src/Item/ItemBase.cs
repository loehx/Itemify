using System;
using Itemify.Core.ItemAccess.Entities;
using Itemify.Core.Keywording;
using Itemify.Core.Typing;
using Itemify.Core.Utils;

namespace Itemify.Core.Item
{
    public class ItemBase : IItem
    {
        private readonly ItemContext context;
        private readonly ItemEntity entity;
        private readonly IItemReference parent;
        private TypeSet subTypes;

        public ItemCollection<IItemReference> Related { get; }
        public ItemCollection<IItemReference> Children { get; }

        public int Revision => entity.Revision;
        public bool Debug => entity.Debug;
        public bool HasBody => !string.IsNullOrEmpty(entity.ValueJson);
        public bool IsParentResolved => Parent is IItem;
        public IItemReference Parent => parent;
        public TypeSet SubTypes => subTypes ?? (subTypes = context.TypeManager.ParseTypeSet(entity.SubTypes ?? ""));
        public DateTime Created => entity.Created;
        public DateTime Modified => entity.Modified;

        internal ItemBase(ItemContext context, ItemEntity entity, IItemReference parent, ItemCollection<IItemReference> related = null, ItemCollection<IItemReference> children = null)
        {
            this.context = context;
            this.entity = entity;
            this.parent = parent;

            Related = related ?? new ItemCollection<IItemReference>();
            Children = children ?? new ItemCollection<IItemReference>();
        }

        public Guid Guid
        {
            get { return entity.Guid; }
            set { entity.Guid = value; }
        }

        public TypeItem Type
        {
            get { return context.TypeManager.ParseTypeItem(entity.Type); }
            set { entity.Type = value.ToStringValue(); }
        }

        public string ParentType
        {
            get { return entity.ParentType; }
            set { entity.ParentType = value; }
        }

        public string Name
        {
            get { return string.IsNullOrEmpty(entity.Name) ? null : entity.Name; }
            set { entity.Name = value ?? ""; }
        }

        public double? ValueNumber
        {
            get { return entity.ValueNumber == double.MinValue ? null : entity.ValueNumber; }
            set
            {
                if (value == double.MinValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "double.MinValue is reserved for Itemify.");

                entity.ValueNumber = value ?? double.MinValue;
            }
        }

        public DateTime? ValueDate
        {
            get { return entity.ValueDate == DateTime.MinValue ? null : entity.ValueDate; }
            set
            {
                if (value == DateTime.MinValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "DateTime.MinValue is reserved for Itemify.");

                entity.ValueDate = value ?? DateTime.MinValue;
            }
        }

        public string ValueString
        {
            get { return string.IsNullOrEmpty(entity.ValueString) ? null : entity.ValueString; }
            set { entity.ValueString = value ?? ""; }
        }

        public int Order
        {
            get { return entity.Order ?? 0; }
            set { entity.Order = value; }
        }

        public T GetBody<T>()
        {
            if (string.IsNullOrEmpty(entity.ValueJson))
                throw new Exception("No body found.");

            return JsonUtil.Parse<T>(entity.ValueJson);
        }

        public T TryGetBody<T>()
        {
            if (string.IsNullOrEmpty(entity.ValueJson))
                return default(T);

            return JsonUtil.TryParse<T>(entity.ValueJson);
        }

        public void SetBody(object body)
        {
            SetBody(body, Debug);
        }

        public void SetBody(object body, bool beatify)
        {
            entity.ValueJson = JsonUtil.Stringify(body, beatify);
            entity.ValueJsonType = body?.GetType().Name ?? "null";
        }


        internal ItemEntity GetEntity()
        {
            if (subTypes != null)
                entity.SubTypes = subTypes.ToStringValue();

            return entity;
        }
    }
}
