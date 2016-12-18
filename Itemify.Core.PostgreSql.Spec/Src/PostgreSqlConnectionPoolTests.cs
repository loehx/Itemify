using System;
using System.Linq;
using System.Threading.Tasks;
using Itemify.Core.PostgreSql.Exceptions;
using Itemify.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Itemify.Core.PostgreSql.Spec
{
    [TestClass()]
    public class PostgreSqlConnectionPoolTests
    {
        private RegionBasedLogWriter logwriter;
        private const string SCHEMA = "tests";

        private const string CONNECTION_STRING =
            "Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;";


        public PostgreSqlConnectionPoolTests()
        {
            var log = new DebugLogData();
            logwriter = new RegionBasedLogWriter(log, nameof(PostgreSqlConnectionPoolTests), 0);
        }



        [Test]
        public void ConnectionPool()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 5000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            Assert.AreEqual(1, providerA.ConnectionId);
            Assert.AreEqual(2, providerB.ConnectionId);
        }

        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void ConnectionPool_ConnectionMaxReached()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            Assert.Throws<TimeoutException>(() =>
            {
                var providerC = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            });
        }

        [Test]
        public void ConnectionPool_Disposing()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 5000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            providerA.Dispose();
            var providerC = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            providerB.Dispose();
            var providerD = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
        }

        [Test]
        public void ConnectionPool_NoTimeout()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            Task.Delay(1000).ContinueWith(k => providerA.Dispose());

            var providerC = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
        }

        [Test]
        public void ConnectionPool_Timeout()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);

            Task.Delay(1100).ContinueWith(k => providerA.Dispose());

            Assert.Throws<TimeoutException>(() =>
            {
                var providerC = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            });
        }
    }
}