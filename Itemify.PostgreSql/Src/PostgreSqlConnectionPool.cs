using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Npgsql;

namespace Itemify.Core.PostgreSql
{
    public class PostgreSqlConnectionPool : IDisposable
    {
        private readonly Queue<NpgsqlConnection> _available = new Queue<NpgsqlConnection>();
        private readonly string _connectionString;
        private readonly int _maxCount;
        private readonly object _syncRoot;
        private readonly int _timeout;

        private const int WaitInterval = 10;

        private int _count;

        public PostgreSqlConnectionPool(string connectionString, int maxCount, int timeoutMilliseconds)
        {
            _connectionString = connectionString;
            _maxCount = maxCount;
            _timeout = timeoutMilliseconds;
            _syncRoot = new object();
            _count = 0;
        }

        public int AvailableCount => _available.Count;
        public int InUseCount => _count - _available.Count;
        public int TotalCount => _count;

        internal PostgreSqlConnectionContext GetContext()
        {
            NpgsqlConnection c;
            var waitTime = 0;

            while (_maxCount == _count && _available.Count == 0)
            {
                if (waitTime >= _timeout)
                    throw new TimeoutException($"No available connection found in connection pool within {_timeout} ms.");

                Thread.Sleep(WaitInterval);
                waitTime += WaitInterval;
            }

            lock (_syncRoot)
            {
                if (_available.Count == 0)
                {
                    c = new NpgsqlConnection(_connectionString);
                    _count++;
                }
                else
                {
                    c =  _available.Dequeue();
                }
            }

            if (c.State != ConnectionState.Open)
                c.Open();

            return new PostgreSqlConnectionContext(c, DisposeConnection);
        }

        internal void DisposeConnection(NpgsqlConnection c)
        {
            lock (_syncRoot)
            {
                _available.Enqueue(c);
            }
        }

        public void Dispose()
        {
            if (_available != null)
            {
                lock (_syncRoot)
                {
                    if (_available.Count != _count)
                        throw new Exception(
                            $"{_count - _available.Count} connections have not been released to the connection pool.");

                    while (_available.Count > 0)
                    {
                        var c = _available.Dequeue();
                        if (c.State == ConnectionState.Open)
                        {
                            c.Close();
                            c.Dispose();
                        }
                    }
                }
            }
        }
    }
}