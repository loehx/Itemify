﻿using System;
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

        public ItemifySettings()
        {
            host = "134.168.62.120";
            port = 5432;
            username = "postgres_dawid";
            password = "LustitiaDev";
            database = "postgres_dawid";
            connectionPoolSize = 50;
            timeout = TimeSpan.FromSeconds(5);

            providerSettings = new ItemProviderSettings
            {
                Timeout = (int)timeout.TotalMilliseconds,
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

        private static string GetConnectionString(string host, int port, string username, string password, string database, int connections)
        {
            return $"Host={host};Port={port};Username={username};Password={password};Database={database};Pooling=true;Minimum Pool Size=0;Maximum Pool Size={connections}";
        }

        internal ItemProviderSettings GetProviderSettings() => providerSettings;
    }
}