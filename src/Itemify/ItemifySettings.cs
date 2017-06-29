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
                PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize)
            };
        }

        public string Host
        {
            get
            {
                return host;
            }
            set
            {
                host = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize);
            }
        }

        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize);
            }
        }

        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize);
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize);
            }
        }

        public string Database
        {
            get
            {
                return database;
            }
            set
            {
                database = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize);
            }
        }

        public int MaxConnections
        {
            get
            {
                return connectionPoolSize;
            }
            set
            {
                connectionPoolSize = value;
                providerSettings.MaxConnections = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(host, port, username, password, database, connectionPoolSize);
            }
        }

        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
                providerSettings.Timeout = timeout;
            }
        }

        public string Schema
        {
            get { return providerSettings.Schema; }
            set { providerSettings.Schema = value; }
        }

        public string PostgreSqlConnectionString => providerSettings.PostgreSqlConnectionString;

        private static string GetConnectionString(string host, int port, string username, string password, string database, int connections)
        {
            return $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Minimum Pool Size=0;Maximum Pool Size={connections}";
        }

        internal ItemProviderSettings GetProviderSettings() => providerSettings;
    }
}