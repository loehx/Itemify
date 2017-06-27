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
        private TimeSpan timeout;

        public ItemifySettings(string host, int port, string username, string password, string database, int connectionPoolSize, TimeSpan timeout)
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
                Timeout = (int)timeout.TotalMilliseconds,
                Schema = "default",
                MaxConnections = connectionPoolSize,
                PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize)
            };
        }

        public string Host
        {
            get
            {
                return Host1;
            }
            set
            {
                Host1 = value;
                providerSettings.PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize);
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
                providerSettings.PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize);
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
                providerSettings.PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize);
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
                providerSettings.PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize);
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
                providerSettings.PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize);
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
                providerSettings.PostgreSqlConnectionString = GetConnectionString(Host1, port, username, password, database, connectionPoolSize);
            }
        }

        public TimeSpan Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
                providerSettings.Timeout = (int) timeout.TotalMilliseconds;
            }
        }

        public string Schema
        {
            get { return providerSettings.Schema; }
            set { providerSettings.Schema = value; }
        }

        public string PostgreSqlConnectionString => providerSettings.PostgreSqlConnectionString;

        public string Host1 { get => host; set => host = value; }

        private static string GetConnectionString(string host, int port, string username, string password, string database, int connections)
        {
            return $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Minimum Pool Size=0;Maximum Pool Size={connections}";
        }

        internal ItemProviderSettings GetProviderSettings() => providerSettings;
    }
}