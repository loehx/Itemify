using System;
using System.Collections.Generic;
using System.Linq;

namespace Itemify.Core.Typing
{
    public class TypeSet : ICloneable
    {
        private readonly TypeManager typeManager;
        private readonly List<TypeItem> _items;

        public bool IsEmpty => _items.Count == 0;

        public TypeSet(TypeManager typeManager)
        {
            this.typeManager = typeManager;
            _items = new List<TypeItem>();
        }

        internal TypeSet(TypeManager typeManager, IEnumerable<TypeItem> items)
        {
            this.typeManager = typeManager;
            this._items = new List<TypeItem>(items);
        }

        public void Set(params Enum[] enumValue)
        {
            foreach (var @enum in enumValue)
            {
                Set(@enum);
            }
        }

        public void Set(Enum enumValue)
        {
            var definition = typeManager.GetDefinitionByType(enumValue.GetType());
            var item = definition.GetItemByEnumValue((int)(object)enumValue);

            Set(item);
        }

        internal void Set(TypeItem item)
        {
            if (_items.Contains(item))
                return;

            _items.Add(item);
        }

        public void Unset<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            var definition = typeManager.GetDefinitionByType(typeof(TEnum));
            var item = definition.GetItemByEnumValue((int)(object)enumValue);

            Unset(item);
        }

        public bool Contains<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            var definition = typeManager.GetDefinitionByType(typeof(TEnum));
            var item = definition.GetItemByEnumValue((int)(object)enumValue);

            return _items.Contains(item);
        }

        internal void Unset(TypeItem item)
        {
            _items.RemoveAll(k => k.Equals(item));
        }

        public override string ToString()
        {
            if (_items.Count == 0)
                return $"No types <{nameof(TypeSet)}>";

            if (_items.Count == 1)
                return $"[{_items[0]}] <{nameof(TypeSet)}>";

            return $"[{_items.Count} types] <{nameof(TypeSet)}>";
        }

        public string ToStringValue()
        {
            var str = _items.Select(k => k.ToStringValue());
            return string.Join("&", str);
        }

        public TypeSet Parse(string source)
        {
            return typeManager.ParseTypeSet(source);
        }

        public TypeSet From(Enum enumValue)
        {
            return typeManager.GetTypeSet(enumValue);
        }

        public override bool Equals(object obj)
        {
            var t = obj as TypeSet;
            if (t != null)
                return Equals(t);

            return false;
        }

        public object Clone()
        {
            return new TypeSet(typeManager, _items);
        }

        public bool Equals(TypeSet typeSet)
        {
            if (typeSet == null)
                return false;

            return typeSet._items.All(k => _items.Contains(k));
        }
    }
}
