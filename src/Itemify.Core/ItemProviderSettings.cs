using System;

namespace Itemify.Core
{
    public class ItemProviderSettings
    {
        public ItemProviderSettings()
        {
        }

        public string PostgreSqlConnectionString { get; set; }
        public int MaxConnections { get; set; }
        public int Timeout { get; set; }
        public string Schema { get; set; }
    }
}