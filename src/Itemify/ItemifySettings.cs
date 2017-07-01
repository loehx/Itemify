using System;
using System.Data.Common;
using Itemify.Core;
namespace Itemify
{
    public class ItemifySettings
    {
        private readonly ItemProviderSettings providerSettings;

        private string host;
        private int port;
        private string username;
        private string password;
        private string database;
        private int connectionPoolSize;
        private int timeout;

        public ItemifySettings()
        {
            providerSettings = new ItemProviderSettings
            {
                Schema = "default"
            };
        }
        
        public ItemifySettings(string host, int port, string username, string password, string database, int connectionPoolSize, int timeout)
        {
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
            this.database = database;
            this.connectionPoolSize = connectionPoolSize;
            this.timeout = timeout;

            providerSettings = new ItemProviderSettings
            {
                Timeout = timeout,
                Schema = "default",
                MaxConnections = connectionPoolSize,
            };
        }

        public string Host
        {
            get => host;
            set => host = value;
        }

        public int Port
        {
            get => port;
            set => port = value;
        }

        public string Username
        {
            get => username;
            set => username = value;
        }

        public string Password
        {
            get => password;
            set => password = value;
        }

        public string Database
        {
            get => database;
            set => database = value;
        }

        public int MaxConnections
        {
            get => connectionPoolSize;
            set
            {
                connectionPoolSize = value;
                providerSettings.MaxConnections = value;
            }
        }

        public int Timeout
        {
            get => timeout;
            set
            {
                timeout = value;
                providerSettings.Timeout = timeout;
            }
        }

        public string Schema
        {
            get => providerSettings.Schema;
            set => providerSettings.Schema = value;
        }

        public string PostgreSqlConnectionString => $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Minimum Pool Size=1;Maximum Pool Size={connectionPoolSize}";

        internal ItemProviderSettings GetProviderSettings()
        {
            providerSettings.PostgreSqlConnectionString = PostgreSqlConnectionString;
            return providerSettings;
        }
    }
}