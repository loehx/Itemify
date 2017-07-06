using System;
using System.Collections;
using Itemify.Core.Keywording;
using Itemify.Core.PostgreSql.Entities;
using Itemify.Shared.Utils;

namespace Itemify.Core.Item
{
    public class 
        
    ItemBase : DefaultItemReference
    {
        private readonly ItemEntity entity;
        private DefaultItemReference parent;

        protected ItemCollection<ItemBase> related { get; }
        protected ItemCollection<ItemBase> children { get; }

        public int Revision => entity.Revision;
        public bool Debug => entity.Debug;
        public bool HasBody => !string.IsNullOrEmpty(entity.ValueJson);
        public bool IsParentResolved => Parent is ItemBase;
        public DefaultItemReference Parent {
            get { return parent; }
            set { parent = value; } }
        public DateTime Created => entity.Created;
        public DateTime Modified => entity.Modified;

        public bool IsNew { get; }

        internal ItemBase(ItemEntity entity, DefaultItemReference parent, bool isNew) 
            : base(entity.Guid, entity.Type)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.entity = entity;
            this.parent = parent;
            this.IsNew = isNew;
            this.related = new ItemCollection<ItemBase>();
            this.children = new ItemCollection<ItemBase>();
        }

        public new Guid Guid
        {
            get => entity.Guid;
            set => entity.Guid = value;
        }

        public new string Type
        {
            get => entity.Type;
            set => entity.Type = value;
        }

        public string Name
        {
//            get => string.IsNullOrEmpty(entity.Name) ? "[" + Type + "]" : entity.Name;
            get => entity.Name;
            set => entity.Name = value ?? "";
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
            entity.ParentGuid = parent.Guid;
            entity.ParentType = parent.Type;

            var now = DateTime.Now;
            now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond)); // cut off ticks

            if (entity.Created == DateTime.MinValue)
            {
                entity.Revision = 0;
                entity.Created = now;
            }
            else
            {
                entity.Revision++;
            }

            entity.Modified = now;

            return entity;
        }

        public override string ToString()
        {
            var creation = Created == DateTime.MinValue ? "NEW" : (DateTime.Now - Created).ToReadableString(1, true) + " ago";
            return $"{Name} <{Type}> ({creation})";
        }

        public override bool Equals(object obj)
        {
            var item = obj as ItemBase;
            if (item != null)
                return Equals(item);

            return false;
        }

        public bool Equals(ItemBase item)
        {
            return item.Guid == Guid && item.Type.Equals(Type);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() ^ Type.GetHashCode();
        }

        public new int CompareTo(object obj)
        {
            var item = obj as ItemBase;
            if (item != null)
            {
                if (IsNew && item.IsNew)
                {
                    return 0;
                }
                else
                {
                    return item.Created.CompareTo(Created);
                }
            }

            if (obj is DefaultItemReference)
                return -1;

            return 0;
        }
    }
}
