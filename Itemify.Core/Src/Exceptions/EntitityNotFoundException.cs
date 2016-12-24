using System;

namespace Itemify.Core.Exceptions
{
    public class EntitityNotFoundException : Exception
    {
        public EntitityNotFoundException(string id, string tableName)
            : base($"No entity with ID \'{id}\' found in table {tableName}.")
        {
        }
    }
}