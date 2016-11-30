using System;
using System.Reflection;
using Itemify.Core.Exceptions;
using Itemify.Core.Utils;

namespace Itemify.Core.Typing
{
    public class TypeItem
    {
        internal TypeDefinition Definition { get; }
        public string Name => Definition.Name;
        public string Value => Inner.Value;
        public int EnumValue => Inner.EnumValue;
        private TypeValue Inner { get; }

        internal TypeItem(TypeDefinition definition, TypeValue inner)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (inner == null) throw new ArgumentNullException(nameof(inner));

            this.Inner = inner;
            Definition = definition;
        }

        internal static TypeItem From<TEnum>(TEnum type)
        {
            var t = typeof(TEnum);
            if (!t.IsEnum)
                throw new ArgumentException($"Parameter {nameof(type)} must be an enum. Actual: {t}");

            var attr = t.GetCustomAttribute(typeof(TypeDefinitionAttribute)) as TypeDefinitionAttribute;
            if (attr == null)
                throw new MissingCustomAttribute($"Type {t} is missing a custom attribute of type {nameof(TypeDefinitionAttribute)}");

            var typeValueAttribute = EnumUtil.GetCustomAttribute<TypeValueAttribute>(type);
            if (typeValueAttribute == null)
                throw new MissingCustomAttribute($"Parameter {nameof(type)} must have a custom attribute of type: {nameof(TypeValueAttribute)}");

            var definition = TypeManager.GetDefinitionByType(t);

            return definition.GetItemByValue(typeValueAttribute.Value);
        }

        public static TypeItem Parse(string source)
        {
            try
            {
                var spl = source.Split('=');
                if (spl.Length != 2)
                    throw new Exception("No seperator found (=).");

                var name = spl[0];
                var value = spl[1];
                var definition = TypeManager.GetDefinitionByName(name);
                var item = definition.GetItemByValue(value);

                return item;
            }
            catch (Exception err)
            {
                throw new Exception($"Unable to parse {nameof(TypeItem)} from: '{source}'", err);
            }
        }

        public override string ToString()
        {
            return $"{Inner.Value}@{Name} <{nameof(TypeItem)}>";
        }

        public string ToStringValue()
        {
            return $"{Name}={Inner.Value}";
        }

        public override bool Equals(object obj)
        {
            var s = obj as string;
            if (s != null)
                return Equals(s);

            var i = obj as TypeItem;
            if (i != null)
                return Equals(i);

            return false;
        }

        public bool Equals(string obj)
        {
            return obj.Equals(Inner.Value, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(TypeItem item)
        {
            return item.Definition.Equals(Definition) && item.Value.Equals(Inner.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Inner.Value.GetHashCode();
        }
    }
}
