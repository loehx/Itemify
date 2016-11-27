using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

        private int count;
        private int poolId;

        private static int connectionPoolCount = 0;

        public PostgreSqlConnectionPool(string connectionString, int maxCount, int timeoutMilliseconds)
        {
            _connectionString = connectionString;
            _maxCount = maxCount;
            _timeout = timeoutMilliseconds;
            _syncRoot = new object();
            count = 0;
            poolId = ++connectionPoolCount;

            write_log($"New pool: {connectionString}");
            write_log($"    size: {maxCount}");
            write_log($"    timeout: {timeoutMilliseconds} ms");
        }

        public int AvailableCount => _available.Count;
        public int InUseCount => count - _available.Count;
        public int TotalCount => count;

        internal PostgreSqlConnectionContext GetContext()
        {
            NpgsqlConnection c;
            var waitTime = 0;

            while (_maxCount == count && _available.Count == 0)
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
                    write_log($"Create new connection #{count + 1} of {_maxCount}");
                    c = new NpgsqlConnection(_connectionString);
                    count++;
                }
                else
                {
                    write_log($"Recycling connection #{_available.Count} of {_available.Count} available (after {waitTime} ms)");
                    c =  _available.Dequeue();
                }
            }

            if (c.State != ConnectionState.Open)
                c.Open();

            return new PostgreSqlConnectionContext(c, DisposeConnection, count);
        }

        internal void DisposeConnection(NpgsqlConnection c, int connectionId)
        {
            lock (_syncRoot)
            {
                write_log($"Connection #{connectionId} released to pool. (available connections: {_available.Count + 1})");
                _available.Enqueue(c);
            }
        }

        public void Dispose()
        {
            if (_available != null)
            {
                lock (_syncRoot)
                {
                    if (_available.Count != count)
                        throw new Exception(
                            $"{count - _available.Count} connections have not been released to the connection pool.");

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

        [Conditional("DEBUG")]
        private void write_log(string message)
        {
            Debug.WriteLine($"[PostgreSqlConnectionPool#{poolId}] " + message);
        }
    }
}