using System;
using System.Linq;
using System.Threading.Tasks;
using Itemify.Core.PostgreSql.Exceptions;
using Itemify.Core.PostgreSql.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Itemify.Core.PostgreSql.Spec
{
    [TestClass()]
    public class PostgreSqlConnectionPoolTests
    {
        private const string SCHEMA = "tests";

        private const string CONNECTION_STRING =
            "Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;";



        [TestMethod()]
        public void ConnectionPool()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 5000);
            var providerA = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);

            Assert.AreEqual(1, providerA.ConnectionId);
            Assert.AreEqual(2, providerB.ConnectionId);
        }

        [TestMethod()]
        [ExpectedException(typeof(TimeoutException))]
        public void ConnectionPool_ConnectionMaxReached()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            var providerC = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
        }

        [TestMethod()]
        public void ConnectionPool_Disposing()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 5000);
            var providerA = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);

            providerA.Dispose();
            var providerC = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);

            providerB.Dispose();
            var providerD = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
        }

        [TestMethod()]
        public void ConnectionPool_NoTimeout()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);

            Task.Delay(1000).ContinueWith(k => providerA.Dispose());

            var providerC = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
        }

        [TestMethod()]
        [ExpectedException(typeof(TimeoutException))]
        public void ConnectionPool_Timeout()
        {
            var connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 2, 1000);
            var providerA = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            var providerB = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);

            Task.Delay(1100).ContinueWith(k => providerA.Dispose());

            var providerC = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
        }
    }
}