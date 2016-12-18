using System;
using System.Linq;
using Itemify.Core.PostgreSql.Exceptions;
using Itemify.Logging;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Itemify.Core.PostgreSql.Spec
{
    [TestFixture]
    public class PostgreSqlProviderTests
    {
        private const string SCHEMA = "tests";
        private const string CONNECTION_STRING = "Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;";
        private PostgreSqlConnectionPool connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 60, 5000);

        private PostgreSqlProvider provider;


        [SetUp]
        public void BeforeEach()
        {
            var log = new DebugLogData();
            var logwriter = new RegionBasedLogWriter(log, nameof(PostgreSqlProviderTests), 0);
            logwriter.StartStopwatch();

            provider = new PostgreSqlProvider(connectionPool, logwriter, SCHEMA);
            provider.EnsureSchemaExists();

            var tables = provider.GetTableNamesBySchema(SCHEMA);
            foreach (var table in tables)
            {
                provider.DropTable(table);
            }
        }

        [TearDown]
        public void AfterEach()
        {
            provider.Dispose();
        }


        [Test]
        public void TableExists()
        {
            var exists = provider.TableExists("not_existing_table");
            Assert.IsFalse(exists);
        }

        [Test]
        public void CreateTable()
        {
            var tableName = "table_a";

            provider.CreateTable<EntityA>(tableName);

            var exists = provider.TableExists(tableName);
            Assert.IsTrue(exists);
        }

        [Test]
        public void Insert_IDefaultEntity()
        {
            var tableName = "table_c";
            var entity = new EntityA()
            {
                Data = new byte[512],
                DateTime = DateTime.Now,
                DateTimeOffset = DateTimeOffset.Now,
                Integer = int.MaxValue,
                NullableDateTime = DateTime.Now,
                NullableInteger = int.MaxValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityA>(tableName);

            var id = provider.Insert(tableName, entity);

            Assert.IsTrue(id > 0);
            Assert.AreEqual(id, entity.Id);
        }


        [Test]
        public void Insert_IGloballyUniqueEntity()
        {
            var tableName = "table_d";
            var entity = new EntityB()
            {
                Data = new byte[] { 0x0, 0x1, 0x2 },
                DateTime = DateTime.MinValue,
                DateTimeOffset = DateTimeOffset.MinValue,
                Integer = int.MinValue,
                NullableDateTime = DateTime.MinValue,
                NullableInteger = int.MinValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityB>(tableName);

            var id = provider.Insert(tableName, entity);

            Assert.IsTrue(id != Guid.Empty);
            Assert.AreEqual(id, entity.Guid);
        }

        [Test]
        public void Insert_IAnonymousEntity()
        {
            var tableName = "table_e";
            var entity = new EntityC()
            {
                Name = "Anonymous item"
            };

            provider.CreateTable<EntityC>(tableName);

            provider.Insert(tableName, entity);
        }


        [Test]
        public void Query_IGloballyUniqueEntity()
        {
            var tableName = "table_d";
            var expected = new EntityB()
            {
                Data = new byte[] { 0x0, 0x1, 0x2 },
                DateTime = DateTime.MinValue,
                DateTimeOffset = DateTimeOffset.MinValue,
                Integer = int.MinValue,
                NullableDateTime = DateTime.MinValue,
                NullableInteger = int.MinValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityB>(tableName);

            var id = provider.Insert(tableName, expected);

            var actual = provider.Query<EntityB>($"SELECT * FROM {provider.ResolveTableName("table_d")} WHERE \"Guid\" = @0", expected.Guid)
                .FirstOrDefault();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Guid);
            Assert.AreEqual(expected.DateTime, actual.DateTime);
            Assert.AreEqual(expected.NullableDateTime, actual.NullableDateTime);
            Assert.AreEqual(expected.DateTimeOffset, actual.DateTimeOffset);
            Assert.AreNotEqual(expected.DateTimeOffset.Offset, actual.DateTimeOffset.Offset, "The offset of DateTimeOffset.MinValue cannot be saved properly.");
            Assert.AreEqual(expected.Integer, actual.Integer);
            Assert.AreEqual(expected.NullableInteger, actual.NullableInteger);
            Assert.AreEqual(expected.String, actual.String);
            Assert.AreEqual(expected.Varchar, actual.Varchar);

            for (var i = 0; i < expected.Data.Length; i++)
            {
                Assert.AreEqual(expected.Data[i], actual.Data[i]);
            }
        }


        [Test]
        public void Query_IGloballyUniqueEntity_SkippingTableResolve()
        {
            var tableName = "table_d";
            var expected = new EntityB()
            {
                Data = new byte[] { 0x0, 0x1, 0x2 },
                DateTime = DateTime.MinValue,
                DateTimeOffset = DateTimeOffset.MinValue,
                Integer = int.MinValue,
                NullableDateTime = DateTime.MinValue,
                NullableInteger = int.MinValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityB>(tableName);
            var id = provider.Insert(tableName, expected);

            Assert.Throws<MissingPropertyException>(() =>
            {
                provider.Query<EntityB>($"SELECT \"Guid\" AS GUID FROM {provider.ResolveTableName(tableName)} WHERE \"Guid\" = @0", expected.Guid)
                    .First();
            });
        }

        [Test]
        public void Query_IGloballyUniqueEntity_NoDeserialization()
        {
            var tableName = "table_d";
            var expected = new EntityB()
            {
                Data = new byte[] { 0x0, 0x1, 0x2 },
                DateTime = DateTime.MinValue,
                DateTimeOffset = DateTimeOffset.MinValue,
                Integer = int.MinValue,
                NullableDateTime = DateTime.MinValue,
                NullableInteger = int.MinValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityB>(tableName);
            var id = provider.Insert(tableName, expected);

            var objects = provider.Query($"SELECT * FROM {provider.ResolveTableName(tableName)} WHERE \"Guid\" = @0", expected.Guid)
                .FirstOrDefault();

            var pos = 0;
            Assert.IsNotNull(objects);
            Assert.AreEqual(expected.Guid, objects[pos++]);
            Assert.AreEqual(expected.DateTime, objects[pos++]);
            Assert.AreEqual(expected.NullableDateTime, objects[pos++]);
            Assert.AreNotEqual(expected.DateTimeOffset, objects[pos++], "Missing: new DateTimeOffset(DateTime.SpecifyKind((DateTime) o, DateTimeKind.Local))");
            Assert.AreEqual(expected.Integer, objects[pos++]);
            Assert.AreEqual(expected.NullableInteger, objects[pos++]);
            Assert.AreEqual(expected.String, objects[pos++]);
            Assert.AreEqual(expected.Varchar, objects[pos++]);
        }

        [Test]
        public void Query_IDefaultEntity()
        {
            var tableName = "table_c";
            var expected = new EntityA()
            {
                Data = new byte[512],
                DateTime = DateTime.Now,
                DateTimeOffset = DateTimeOffset.Now,
                Integer = int.MaxValue,
                NullableDateTime = DateTime.Now,
                NullableInteger = int.MaxValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityA>(tableName);

            var id = provider.Insert(tableName, expected);

            var actual = provider.Query<EntityA>($"SELECT * FROM {provider.ResolveTableName("table_c")} WHERE \"Id\" = @0", expected.Id)
                .FirstOrDefault();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Type, actual.Type);
            Assert.IsTrue(expected.DateTime.Subtract(actual.DateTime) < TimeSpan.FromMilliseconds(1));
            Assert.IsTrue(actual.NullableDateTime.HasValue);
            Assert.IsTrue(expected.NullableDateTime.Value.Subtract(actual.NullableDateTime.Value) < TimeSpan.FromMilliseconds(1));
            Assert.IsTrue(expected.DateTimeOffset.Subtract(actual.DateTimeOffset) < TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(expected.DateTimeOffset.Offset, actual.DateTimeOffset.Offset);
            Assert.AreEqual(expected.Integer, actual.Integer);
            Assert.AreEqual(expected.NullableInteger, actual.NullableInteger);
            Assert.AreEqual(expected.String, actual.String);
            Assert.AreEqual(expected.Varchar, actual.Varchar);
            for (var i = 0; i < expected.Data.Length; i++)
            {
                Assert.AreEqual(expected.Data[i], actual.Data[i]);
            }
        }

        [Test]
        public void Upsert_IDefaultEntity_NoMerge()
        {
            var tableName = "table_c";
            var firstEntity = new EntityA()
            {
                Data = new byte[512],
                DateTime = DateTime.Now,
                DateTimeOffset = DateTimeOffset.Now,
                Integer = int.MaxValue,
                NullableDateTime = DateTime.Now,
                NullableInteger = int.MaxValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityA>(tableName);
            var id = provider.Insert(tableName, firstEntity);

            var expected = new EntityA()
            {
                Id = id,
                Data = new byte[12],
                DateTime = DateTime.Today,
                DateTimeOffset = DateTime.Today,
                Integer = 0,
                NullableDateTime = null,
                NullableInteger = 123,
                String = null,
                Varchar = "Test"
            };

            var id2 = provider.Insert(tableName, expected, upsert: true, merge: false);
            Assert.AreEqual(id, id2);

            var actual = provider.Query<EntityA>($"SELECT * FROM {provider.ResolveTableName("table_c")} WHERE \"Id\" = @0", expected.Id)
                .FirstOrDefault();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Type, actual.Type); 
            Assert.IsTrue(expected.DateTime.Subtract(actual.DateTime) < TimeSpan.FromMilliseconds(1));
            Assert.IsFalse(actual.NullableDateTime.HasValue);
            Assert.IsFalse(expected.NullableDateTime.HasValue);
            Assert.IsTrue(expected.DateTimeOffset.Subtract(actual.DateTimeOffset) < TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(expected.DateTimeOffset.Offset, actual.DateTimeOffset.Offset);
            Assert.AreEqual(expected.Integer, actual.Integer);
            Assert.AreEqual(expected.NullableInteger, actual.NullableInteger);
            Assert.AreEqual(expected.String, actual.String);
            Assert.AreEqual(expected.Varchar, actual.Varchar);
            for (var i = 0; i < expected.Data.Length; i++)
            {
                Assert.AreEqual(expected.Data[i], actual.Data[i]);
            }
        }


        [Test]
        public void Upsert_IDefaultEntity_Merge()
        {
            var tableName = "table_c";
            var firstEntity = new EntityA()
            {
                Data = new byte[512],
                NullableDateTime = DateTime.Now,
                NullableInteger = int.MaxValue,
                String = new string('S', 120),
                Varchar = new string('S', 50)
            };

            provider.CreateTable<EntityA>(tableName);
            var id = provider.Insert(tableName, firstEntity);

            var expected = new EntityA()
            {
                Id = id,
                Data = null,
                NullableDateTime = null,
                NullableInteger = null,
                String = null,
                Varchar = null
            };

            provider.Insert(tableName, expected, upsert: true, merge: true); // merge = true
            var actual = provider.Query<EntityA>($"SELECT * FROM {provider.ResolveTableName("table_c")} WHERE \"Id\" = @0", expected.Id)
                .FirstOrDefault();

            Assert.AreEqual(firstEntity.Type, actual.Type);
            Assert.IsTrue(firstEntity.DateTime.Subtract(actual.DateTime) < TimeSpan.FromMilliseconds(1));
            Assert.IsTrue(actual.NullableDateTime.HasValue);
            Assert.IsTrue(firstEntity.NullableDateTime.HasValue);
            Assert.AreEqual(firstEntity.NullableInteger, actual.NullableInteger);
            Assert.AreEqual(firstEntity.String, actual.String);
            Assert.AreEqual(firstEntity.Varchar, actual.Varchar);
        }
    }

    public class EntityA : IDefaultEntity
    {
        [PostgreSqlColumn(dataType: "SERIAL", primaryKey: true)]
        public int Id { get; set; }

        [PostgreSqlColumn(name: "type")]
        public int Type => 0;

        [PostgreSqlColumn()]
        public DateTime DateTime { get; set; }

        [PostgreSqlColumn]
        public DateTime? NullableDateTime { get; set; }

        [PostgreSqlColumn]
        public DateTimeOffset DateTimeOffset { get; set; }

        [PostgreSqlColumn]
        public int Integer { get; set; }

        [PostgreSqlColumn]
        public int? NullableInteger { get; set; }

        [PostgreSqlColumn]
        public string String { get; set; }

        [PostgreSqlColumn(dataType: "varchar(50)")]
        public string Varchar { get; set; }

        [PostgreSqlColumn]
        public byte[] Data { get; set; }
    }

    public class EntityB : IGloballyUniqueEntity
    {
        [PostgreSqlColumn(primaryKey: true)]
        public Guid Guid { get; set; }

        [PostgreSqlColumn]
        public DateTime DateTime { get; set; }

        [PostgreSqlColumn]
        public DateTime? NullableDateTime { get; set; }

        [PostgreSqlColumn]
        public DateTimeOffset DateTimeOffset { get; set; }

        [PostgreSqlColumn]
        public int Integer { get; set; }

        [PostgreSqlColumn]
        public int? NullableInteger { get; set; }

        [PostgreSqlColumn]
        public string String { get; set; }

        [PostgreSqlColumn(dataType: "varchar(50)")]
        public string Varchar { get; set; }

        [PostgreSqlColumn]
        public byte[] Data { get; set; }
    }


    public class EntityC : IAnonymousEntity
    {
        [PostgreSqlColumn]
        public string Name { get; set; }
    }
}