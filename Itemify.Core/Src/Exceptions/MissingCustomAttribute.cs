using System;

namespace Itemify.Core.Exceptions
{
    public class MissingCustomAttribute : Exception
    {
        public MissingCustomAttribute(string message) 
            : base(message)
        {
        }
    }

    public class MissingCustomAttribute<T> : MissingCustomAttribute
        where T: Attribute
    {
        public MissingCustomAttribute()
            : base($"Missing custom attribute {typeof(T).Name}.")
        {
        }
    }
}
