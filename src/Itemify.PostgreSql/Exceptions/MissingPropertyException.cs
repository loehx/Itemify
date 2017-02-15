using System;

namespace Itemify.Core.PostgreSql.Exceptions
{
    public class MissingPropertyException : Exception
    {
        public MissingPropertyException(Type type, string name)
            : base($"Missing property '{name}' in type: {type.Name}")
        {
        }
    }
}