using System;
using Itemify.Core;
using Itemify.Core.Typing;

namespace Itemify
{
    public class ItemifySettings
    {
        private readonly ItemProviderSettings providerSettings;
        private readonly TypeManager typeManager;

        public ItemifySettings(string postgreSqlConnectionString)
        {
            this.typeManager = new TypeManager();
            this.providerSettings = new ItemProviderSettings(typeManager)
            {
                Timeout = 5000,
                Schema = "default",
                MaxConnections = 50,
                PostgreSqlConnectionString = postgreSqlConnectionString
            };
        }

        internal TypeManager GetTypeManager() => typeManager;

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

        public void Register<T>() where T : struct
        {
            typeManager.Register<T>();
        }

        public void Register(Type type)
        {
            typeManager.Register(type);
        }

        internal ItemProviderSettings GetProviderSettings() => providerSettings;
    }
}