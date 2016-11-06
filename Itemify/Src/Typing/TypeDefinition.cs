using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Itemify.Exceptions;

namespace Itemify.Typing
{
    public class TypeDefinition
    {
        private readonly List<TypeItem> _items;

        private readonly TypeDefinitionAttribute _inner;
        private readonly Type _type;

        internal TypeDefinition(TypeDefinitionAttribute attr, Type type)
        {
            if (attr == null) throw new ArgumentNullException(nameof(attr));
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!Regex.IsMatch(attr.Name, "^[A-Za-z0-9]+$"))
                throw new ArgumentException($"Name of {nameof(TypeDefinition)} cannot contain special characters: '{attr.Name}'");

            _type = type;
            _inner = attr;

            _items = GetItems().ToList();
        }

        public string Name => _inner.Name;
        public IEnumerable<TypeItem> Items => _items;

        private IEnumerable<TypeItem> GetItems()
        {
            var fields = _type.GetFields(BindingFlags.Public | BindingFlags.Static);
            var typeValues = new List<TypeValue>(fields.Length);

            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<TypeValueAttribute>();
                if (attr == null)
                    throw new MissingCustomAttribute($"Enum field '{field}' in type '{_type.Name}' is missing a custom attribute of type: {nameof(TypeValueAttribute)}");

                var typeValue = new TypeValue(attr, (int) field.GetValue(null));
                if (typeValues.Contains(typeValue))
                    throw new DuplicateNameException($"Duplicate '{nameof(TypeValue)}' in enum '{_type.Name}': '{typeValue.Value}'");

                typeValues.Add(typeValue);

                yield return new TypeItem(this, typeValue);
            }
        }

        public TypeItem GetItemByValue(string value)
        {
            var item = _items.FirstOrDefault(k => k.Equals(value));
            if (item == null)
                throw new Exception($"Value could not be found in '{this}': '{value}'");

            return item;
        }

        public TypeItem GetItemByEnumValue(int value)
        {
            var item = _items.FirstOrDefault(k => k.EnumValue == value);
            if (item == null)
                throw new Exception($"Enum value could not be found in '{this}': '{value}'");

            return item;
        }

        public override string ToString()
        {
            return $"{_inner.Name} ({_items.Count} items) <{nameof(TypeDefinition)}>";
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeDefinition)
                return Equals(obj as TypeDefinition);

            return false;
        }

        public bool Equals(TypeDefinition typeDefinition)
        {
            return typeDefinition.Name.Equals(Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
