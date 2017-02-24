using System;
using Itemify.Core.Typing;

namespace Itemify.Core
{
    public class ItemProviderSettings
    {
        private readonly TypeManager typeManager;

        public ItemProviderSettings(TypeManager typeManager)
        {
            this.typeManager = typeManager;
        }

        public string PostgreSqlConnectionString { get; set; }
        public int MaxConnections { get; set; }
        public int Timeout { get; set; }
        public string Schema { get; set; }
    }
}