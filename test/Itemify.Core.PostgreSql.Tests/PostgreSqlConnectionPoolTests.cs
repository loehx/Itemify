using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Itemify.Core.PostgreSql.Exceptions;
using Itemify.Logging;
using Itemify.Shared.Utils;
using Xunit;

namespace Itemify.Core.PostgreSql.Spec
{
    public class PostgreSqlConnectionPoolTests
    {
        private ILogWriter logwriter;
        private const string SCHEMA = "tests";

        private const string CONNECTION_STRING =
            "Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;";


        public PostgreSqlConnectionPoolTests()
        {
            var log = new CustomLogData(l => Debug.WriteLine(l));
            logwriter = new RegionBasedLogWriter(log, nameof(PostgreSqlConnectionPoolTests));
        }


        [Fact]
        public void ConnectionPool_ParallelQuery()
        {
            var connectionPool = PostgreSqlConnectionPoolFactory.GetPoolByConnectionString(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            10000.Times()
                .AsParallel()
                .WithDegreeOfParallelism(20)
                .ForAll(k =>
            {
                providerA.Query("SELECT 1");
                providerB.Query("SELECT 2");
            });
        }

        [Fact]
        public void ConnectionPool_Disposing()
        {
            var connectionPool = PostgreSqlConnectionPoolFactory.GetPoolByConnectionString(CONNECTION_STRING, 2, 5000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            providerA.Dispose();
            var providerC = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            providerB.Dispose();
            var providerD = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
        }

        [Fact]
        public void ConnectionPool_NoTimeout()
        {
            var connectionPool = PostgreSqlConnectionPoolFactory.GetPoolByConnectionString(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            Task.Delay(1000).ContinueWith(k => providerA.Dispose());

            var providerC = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
        }
    }
}