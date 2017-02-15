using System;
using System.Reflection;
using Itemify.Core.Exceptions;

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

        public bool Equals(Enum e)
        {
            return true;
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
