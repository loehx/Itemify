using System;
using System.Collections.Generic;
using System.Linq;

namespace Itemify.Core.Typing
{
    public class TypeSet : ICloneable
    {
        private readonly List<TypeItem> _items;

        public TypeSet()
        {
            _items = new List<TypeItem>();
        }

        internal TypeSet(IEnumerable<TypeItem> items)
        {
            this._items = new List<TypeItem>(items);
        }

        public void Set<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            var definition = TypeManager.GetDefinitionByType(typeof(TEnum));
            var item = definition.GetItemByEnumValue((int)(object)enumValue);

            Set(item);
        }

        public void Unset<TEnum>(TEnum enumValue, bool @on = true)
            where TEnum : struct
        {
            var definition = TypeManager.GetDefinitionByType(typeof(TEnum));
            var item = definition.GetItemByEnumValue((int)(object)enumValue);

            Unset(item);
        }

        internal void Set(TypeItem item)
        {
            if (_items.Count > 0)
                _items.RemoveAll(k => k.Definition.Equals(item.Definition)); // Remove all items having the same definition

            _items.Add(item);
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

        public static TypeSet Parse(string source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var items = source.Split('&').Select(TypeItem.Parse);
            return new TypeSet(items);
        }

        public static TypeSet From<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            var set = new TypeSet();
            set.Set(enumValue);
            return set;
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
            return new TypeSet(_items);
        }

        public bool Equals(TypeSet typeSet)
        {
            if (typeSet == null)
                return false;

            return typeSet._items.All(k => _items.Contains(k));
        }
    }
}
