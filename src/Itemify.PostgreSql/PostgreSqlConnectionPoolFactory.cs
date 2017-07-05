using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Itemify.Core.PostgreSql
{
    public class PostgreSqlConnectionPoolFactory
    {
        private static Hashtable pools = new Hashtable();

        public static int PoolCount => pools.Count;
        public static IEnumerable<PostgreSqlConnectionPool> Pools => Enumerable.Cast<PostgreSqlConnectionPool>(pools.Values);
        public static object syncRoot = new object();

        public static PostgreSqlConnectionPool GetPoolByConnectionString(string connectionString, int maxCount, int timeoutMilliseconds)
        {
            lock (syncRoot)
            {
                return pools[connectionString] as PostgreSqlConnectionPool
                       ?? (PostgreSqlConnectionPool) (pools[connectionString] =
                           new PostgreSqlConnectionPool(connectionString, maxCount, timeoutMilliseconds));
            }
        }
    }
}