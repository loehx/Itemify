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
        private string sslMode;
        private bool trustServerCertificate;

        public ItemifySettings()
        {
            providerSettings = new ItemProviderSettings
            {
                Schema = "default"
            };
        }
        
        public ItemifySettings(string host, int port, string username, string password, string database, int connectionPoolSize, int timeout, string sslMode, bool trustServerCertificate)
        {
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;
            this.database = database;
            this.connectionPoolSize = connectionPoolSize;
            this.timeout = timeout;
            this.sslMode = sslMode;
            this.trustServerCertificate = trustServerCertificate;

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

        public string SslMode
        {
            get => sslMode;
            set => sslMode = value;
        }
        public bool TrustServerCertificate
        {
            get => trustServerCertificate;
            set => trustServerCertificate = value;
        }

        public string PostgreSqlConnectionString => $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Minimum Pool Size=1;Maximum Pool Size={connectionPoolSize};SSL Mode={SslMode};Trust Server Certificate={TrustServerCertificate}";

        internal ItemProviderSettings GetProviderSettings()
        {
            providerSettings.PostgreSqlConnectionString = PostgreSqlConnectionString;
            return providerSettings;
        }
    }
}