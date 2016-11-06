using System;

namespace Itemify.Typing
{
    public class TypeValueAttribute : Attribute
    {
        public TypeValueAttribute(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Parameter {nameof(value)} cannot be empty or whitespace.");

            Value = value;
        }

        public string Value { get; }
    }
}
