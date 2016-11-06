using System;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    internal class PostgreSqlConnectionContext : IDisposable
    {
        private readonly Action<NpgsqlConnection> _onDispose;
        public NpgsqlConnection Connection { get; }

        public PostgreSqlConnectionContext(NpgsqlConnection c, Action<NpgsqlConnection> onDispose)
        {
            _onDispose = onDispose;
            this.Connection = c;
        }

        public void Dispose()
        {
            _onDispose(Connection);
        }
    }
}