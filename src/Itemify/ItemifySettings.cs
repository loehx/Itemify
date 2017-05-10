using System;
using Itemify.Core;
namespace Itemify
{
    public class ItemifySettings
    {
        private readonly ItemProviderSettings providerSettings;

        public ItemifySettings(string postgreSqlConnectionString)
        {
            this.providerSettings = new ItemProviderSettings()
            {
                Timeout = 5000,
                Schema = "default",
                MaxConnections = 50,
                PostgreSqlConnectionString = postgreSqlConnectionString
            };
        }

        public string PostgreSqlConnectionString
        {
            get { return providerSettings.PostgreSqlConnectionString; }
            set { providerSettings.PostgreSqlConnectionString = value; }
        }

        public int MaxConnections
        {
            get { return providerSettings.MaxConnections; }
            set { providerSettings.MaxConnections = value; }
        }

        public int Timeout
        {
            get { return providerSettings.Timeout; }
            set { providerSettings.Timeout = value; }
        }

        public string Schema
        {
            get { return providerSettings.Schema; }
            set { providerSettings.Schema = value; }
        }

        internal ItemProviderSettings GetProviderSettings() => providerSettings;
    }
}