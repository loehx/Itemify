using System;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    internal class PostgreSqlConnectionContext : IDisposable
    {
        private readonly Action<NpgsqlConnection, int> _onDispose;
        public NpgsqlConnection Connection { get; }
        public int ConnectionId { get; }

        public PostgreSqlConnectionContext(NpgsqlConnection c, Action<NpgsqlConnection, int> onDispose, int connectionId)
        {
            _onDispose = onDispose;
            ConnectionId = connectionId;
            this.Connection = c;
        }

        public void Dispose()
        {
            _onDispose(Connection, ConnectionId);
        }
    }
}