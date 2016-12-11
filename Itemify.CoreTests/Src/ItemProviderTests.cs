﻿using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql;
using Itemify.Core.PostgreSql.Logging;
using Itemify.Core.Src.PostgreSql;
using Itemify.Core.Typing;
using Itemify.Spec.Example_A.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Itemify.Core.Spec
{
    [TestClass()]
    public class ItemProviderTests
    {
        private const string SCHEMA = "spec";
        private const string CONNECTION_STRING = "Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;";
        private PostgreSqlConnectionPool connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 60, 5000);
        private PostgreSqlProvider sqlProvider;
        private EntityProvider entityProvider;
        private ItemProvider provider;
        private TypeManager typeManager;

        [TestInitialize]
        public void BeforeEach()
        {
            sqlProvider = new PostgreSqlProvider(connectionPool, new DebuggingSqlLog(), SCHEMA);
            sqlProvider.EnsureSchemaExists();

            typeManager = new TypeManager();
            typeManager.Register<DeviceType>();
            typeManager.Register<SensorType>();

            entityProvider = new EntityProvider(sqlProvider, new EntityProviderLog(s => Console.WriteLine("[EntityProvider] " + s)));
            provider = new ItemProvider(entityProvider, typeManager);

            var tables = sqlProvider.GetTableNamesBySchema(SCHEMA);
            foreach (var table in tables)
            {
                sqlProvider.DropTable(table);
            }
        }

        [TestCleanup]
        public void AfterEach()
        {
            sqlProvider.Dispose();
        }


        [TestMethod()]
        public void RootItem()
        {
            Assert.IsNotNull(provider.Root);
            Assert.AreEqual(provider.Root.Guid, Guid.Empty);
            Assert.AreEqual(provider.Root.Type.Name, "i");
            Assert.AreEqual(provider.Root.Type.Value, "root");
            Assert.AreEqual(provider.Root.Type.EnumValue, 0);
        }


        [TestMethod()]
        public void NewItem_A()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Sensor));

            Assert.IsNotNull(item);
            Assert.AreNotEqual(Guid.Empty, item.Guid);
            Assert.AreEqual(item.Type, typeManager.GetTypeItem(DeviceType.Sensor));
            Assert.AreEqual(item.Children.Count, 0);
            Assert.AreEqual(item.Related.Count, 0);
            Assert.AreEqual(item.Created, DateTime.MinValue);
            Assert.AreEqual(item.Modified, DateTime.MinValue);
            Assert.IsFalse(item.IsParentResolved);
            Assert.AreEqual(item.Debug, false);
            Assert.AreEqual(item.Name, null);
            Assert.AreEqual(item.Order, 0);
            Assert.AreEqual(item.Parent, provider.Root);
            Assert.AreEqual(item.Revision, 0);
            Assert.AreEqual(item.ValueString, null);
            Assert.AreEqual(item.ValueNumber, null);
            Assert.AreEqual(item.ValueDate, null);
            Assert.AreEqual(item.TryGetBody<object>(), null);
        }

        private class ExampleBodyA
        {
            public string StringValue { get; set; }
            public decimal DecimalValue { get; set; }
            public double DoubleValue { get; set; }
            public DateTime DateTimeValue { get; set; }
            public TimeSpan TimeSpanValue { get; set; }
            public int IntValue { get; set; }
            public bool BooleanValue { get; set; }
            public byte[] BinaryValue { get; set; }
            public object ComplexValue { get; set; }
            public IReadOnlyList<int> Collection { get; set; }

            public class ComplexTypeA
            {
                public string StringValue { get; set; }
            }
            public class ComplexTypeB
            {
                public double DecimalValue { get; set; }
            }
        }

        [TestMethod()]
        public void NewItem_B()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.Name = "Example item";
            item.Order = int.MinValue;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = double.MinValue * 1.1;
            item.ValueString = new string('E', 1024 * 1024);

            Assert.AreNotEqual(Guid.Empty, item.Guid);
            Assert.AreEqual(item.Type, typeManager.GetTypeItem(DeviceType.Meter));
            Assert.AreEqual(item.Children.Count, 0);
            Assert.AreEqual(item.Related.Count, 0);
            Assert.AreEqual(item.Created, DateTime.MinValue);
            Assert.AreEqual(item.Modified, DateTime.MinValue);
            Assert.IsFalse(item.IsParentResolved);
            Assert.AreEqual(item.Debug, false);
            Assert.AreEqual(item.Name, "Example item");
            Assert.AreEqual(item.Order, int.MinValue);
            Assert.AreEqual(item.Parent, provider.Root);
            Assert.AreEqual(item.Revision, 0);
            Assert.AreEqual(item.ValueString.Length, 1024 * 1024);
            Assert.AreEqual(item.ValueString, new string('E', 1024 * 1024));
            Assert.AreEqual(item.ValueNumber, double.MinValue * 1.1);
            Assert.AreEqual(item.ValueDate, DateTime.MinValue.AddMilliseconds(1));
            Assert.IsTrue(item.SubTypes.IsEmpty);
        }

        [TestMethod()]
        public void NewItem_C()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Sensor));

            item.Name = "";
            item.Order = int.MaxValue;
            item.ValueDate = DateTime.MaxValue;
            item.ValueNumber = double.MaxValue;
            item.ValueString = "";
            item.SubTypes.Set(SensorType.Brightness);
            item.SubTypes.Set(SensorType.SetTemperature);

            Assert.AreNotEqual(Guid.Empty, item.Guid);
            Assert.AreEqual(item.Type, typeManager.GetTypeItem(DeviceType.Sensor));
            Assert.AreEqual(item.Children.Count, 0);
            Assert.AreEqual(item.Related.Count, 0);
            Assert.AreEqual(item.Created, DateTime.MinValue);
            Assert.AreEqual(item.Modified, DateTime.MinValue);
            Assert.IsFalse(item.IsParentResolved);
            Assert.AreEqual(item.Debug, false);
            Assert.AreEqual(item.Name, null);
            Assert.AreEqual(item.Order, int.MaxValue);
            Assert.AreEqual(item.Parent, provider.Root);
            Assert.AreEqual(item.Revision, 0);
            Assert.AreEqual(item.ValueString, null);
            Assert.AreEqual(item.ValueNumber, double.MaxValue);
            Assert.AreEqual(item.ValueDate, DateTime.MaxValue);
            Assert.IsFalse(item.SubTypes.IsEmpty);
            Assert.IsTrue(item.SubTypes.Contains(SensorType.Brightness));
            Assert.IsTrue(item.SubTypes.Contains(SensorType.SetTemperature));
            Assert.IsFalse(item.SubTypes.Contains(SensorType.Temperature));
        }

        [TestMethod()]
        public void NewItem_BodyA()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            var body = new ExampleBodyA()
            {
                BinaryValue = new byte[] { 0x23, 0x0, 0xff },
                BooleanValue = true,
                ComplexValue = new ExampleBodyA.ComplexTypeA() { StringValue = "Test" },
                StringValue = "Test",
                DateTimeValue = DateTime.MinValue,
                DecimalValue = decimal.MinValue,
                DoubleValue = double.MinValue,
                IntValue = int.MinValue,
                TimeSpanValue = TimeSpan.MinValue,
                Collection = new List<int>() { 1, 2, 3, 4 }
            };

            item.SetBody(body, true);

            var actualBody = item.GetBody<ExampleBodyA>();
            Assert.AreEqual(actualBody.StringValue, body.StringValue);
            Assert.AreEqual(actualBody.BooleanValue, body.BooleanValue);
            Assert.AreEqual(actualBody.BinaryValue[0], body.BinaryValue[0]);
            Assert.AreEqual(actualBody.BinaryValue[1], body.BinaryValue[1]);
            Assert.AreEqual(actualBody.BinaryValue[2], body.BinaryValue[2]);
            Assert.AreEqual(actualBody.DateTimeValue, body.DateTimeValue);
            Assert.AreEqual(actualBody.DecimalValue, body.DecimalValue);
            Assert.AreEqual(actualBody.DoubleValue, body.DoubleValue);
            Assert.AreEqual(actualBody.IntValue, body.IntValue);
            Assert.AreEqual(actualBody.TimeSpanValue, body.TimeSpanValue);
            Assert.AreEqual(actualBody.Collection[0], body.Collection[0]);
            Assert.AreEqual(actualBody.Collection[1], body.Collection[1]);
            Assert.AreEqual(actualBody.Collection[2], body.Collection[2]);
            Assert.AreEqual(actualBody.Collection[3], body.Collection[3]);

            Assert.AreEqual(((ExampleBodyA.ComplexTypeA)actualBody.ComplexValue).StringValue, ((ExampleBodyA.ComplexTypeA)actualBody.ComplexValue).StringValue);
        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NewItem_BadNumberValue()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.ValueNumber = double.MinValue; // reserved by Itemify
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NewItem_BadDateValue()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.ValueDate = DateTime.MinValue; // reserved by Itemify
        }

        [TestMethod()]
        public void SaveItem()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            Assert.Fail("Continue here ...");
        }
    }
}