using System;
using System.Text.RegularExpressions;

namespace Itemify.Core.Typing
{
    public class TypeValue
    {
        private readonly TypeValueAttribute _value;

        internal TypeValue(TypeValueAttribute value, int enumValue)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (!Regex.IsMatch(value.Value, "^[A-Za-z0-9]+$"))
                throw new ArgumentException($"Name of {nameof(TypeValueAttribute)} cannot contain special characters: '{value.Value}'");

            _value = value;
            EnumValue = enumValue;
        }

        public string Value => _value.Value;
        public int EnumValue { get; }

        public override string ToString()
        {
            return $"{_value.Value}: {EnumValue} <{nameof(TypeValue)}>";
        }

        public override bool Equals(object obj)
        {
            var t = obj as TypeValue;
            if (t != null)
                return Equals(t);

            return false;
        }

        public bool Equals(TypeValue obj)
        {
            if (obj == null)
                return false;

            return obj.Value.Equals(Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return _value.Value.GetHashCode() ^ EnumValue.GetHashCode();
        }
    }
}