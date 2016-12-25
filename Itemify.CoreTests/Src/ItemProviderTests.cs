using System;
using System.Collections.Generic;
using System.Linq;
using Itemify.Core.Exceptions;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql;
using Itemify.Core.Typing;
using Itemify.Logging;
using Itemify.Shared.Logging;
using Itemify.Spec.Example_A.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

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
        private RegionBasedLogWriter logwriter;

        [SetUp]
        public void BeforeEach()
        {
            var log = new DebugLogData();
            logwriter = new RegionBasedLogWriter(log, nameof(ItemProviderTests), 0);
            sqlProvider = new PostgreSqlProvider(connectionPool, logwriter.NewRegion(nameof(PostgreSqlProvider)), SCHEMA);
            sqlProvider.EnsureSchemaExists();

            typeManager = new TypeManager();
            typeManager.Register<DeviceType>();
            typeManager.Register<SensorType>();

            entityProvider = new EntityProvider(sqlProvider, logwriter.NewRegion(nameof(EntityProvider)));
            provider = new ItemProvider(entityProvider, typeManager, logwriter);

            var tables = sqlProvider.GetTableNamesBySchema(SCHEMA);
            foreach (var table in tables)
            {
                sqlProvider.DropTable(table);
            }
        }

        [TearDown]
        public void AfterEach()
        {
            sqlProvider.Dispose();
            logwriter.Dispose();
        }


        [Test]
        public void RootItem()
        {
            Assert.IsNotNull(provider.Root);
            Assert.AreEqual(provider.Root.Guid, Guid.Empty);
            Assert.AreEqual(provider.Root.Type.Name, "i");
            Assert.AreEqual(provider.Root.Type.Value, "root");
            Assert.AreEqual(provider.Root.Type.EnumValue, 0);
        }


        [Test]
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
            Assert.AreEqual(item.Name, "DeviceType");
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

        [Test]
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

        [Test]
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
            Assert.AreEqual(item.Name, "DeviceType");
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

        [Test]
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


        [Test]
        public void NewItem_BadNumberValue()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            Assert.Throws<ArgumentOutOfRangeException>(() => item.ValueNumber = double.MinValue); // reserved by Itemify
        }

        [Test]
        public void NewItem_BadDateValue()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            Assert.Throws<ArgumentOutOfRangeException>(() => item.ValueDate = DateTime.MinValue); // reserved by Itemify
        }

        [Test]
        public void SaveItem()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

            var id = provider.Save(item);
            Assert.AreEqual(id, item.Guid);
        }


        [Test]
        public void SaveAndGetItem()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

            var id = provider.Save(item);

            var actual = provider.GetItemByReference(item);

            Assert.AreNotEqual(Guid.Empty, actual.Guid);
            Assert.AreEqual(id, actual.Guid);
            Assert.AreEqual(actual.Type, typeManager.GetTypeItem(DeviceType.Meter));
            Assert.AreEqual(actual.Children.Count, 0);
            Assert.AreEqual(actual.Related.Count, 0);
            Assert.AreEqual(actual.Created, item.Created);
            Assert.AreEqual(actual.Modified, item.Modified);
            Assert.IsFalse(actual.IsParentResolved);
            Assert.AreEqual(actual.Debug, false);
            Assert.AreEqual(actual.Name, "Example");
            Assert.AreEqual(actual.Order, -1);
            Assert.AreEqual(actual.Parent, provider.Root);
            Assert.AreEqual(actual.Revision, 0);
            Assert.AreEqual(actual.ValueString, "string");
            Assert.AreEqual(actual.ValueNumber, 1.1);
            Assert.AreEqual(actual.ValueDate, DateTime.MinValue.AddMilliseconds(1));
            Assert.IsTrue(actual.SubTypes.IsEmpty);
        }



        [Test]
        public void SaveAndGetItem_SetValuesToNull()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";
            item.SubTypes.Set(SensorType.Brightness, SensorType.SetTemperature);


            var id = provider.Save(item);
            var saved = provider.GetItemByReference(item);

            saved.ValueDate = null;
            saved.ValueNumber = null;
            saved.ValueString = null;
            saved.SetBody(null);
            saved.Name = "New name";
            saved.Order = 5;

            provider.Save(saved);
            var final = provider.GetItemByReference(item);

            Assert.AreEqual(id, final.Guid);
            Assert.AreEqual(1, final.Revision);
            Assert.AreEqual("New name", final.Name);
            Assert.AreEqual(5, final.Order);
            Assert.AreEqual(null, final.ValueDate);
            Assert.AreEqual(null, final.ValueNumber);
            Assert.AreEqual(null, final.ValueString);
            Assert.AreEqual(0, final.Children.Count);
            Assert.AreEqual(0, final.Related.Count);
            Assert.AreEqual(item.Created, final.Created);
            Assert.AreNotEqual(item.Modified, final.Modified);
            Assert.AreEqual(item.SubTypes, final.SubTypes);
            Assert.AreEqual(item.Type, final.Type);
            Assert.AreEqual(null, final.GetBody<string>());
        }



        [Test]
        public void SaveAndGetItem_MinimumInformation()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));
            var id = provider.Save(item);
            var saved = provider.GetItemByReference(item);
           
            Assert.AreEqual(id, saved.Guid);
            Assert.AreEqual(0, saved.Revision);
            Assert.AreEqual("DeviceType", saved.Name);
            Assert.AreEqual(0, saved.Order);
            Assert.AreEqual(null, saved.ValueDate);
            Assert.AreEqual(null, saved.ValueNumber);
            Assert.AreEqual(null, saved.ValueString);
            Assert.AreEqual(0, saved.Children.Count);
            Assert.AreEqual(0, saved.Related.Count);
            Assert.AreEqual(item.Created, saved.Created);
            Assert.AreEqual(item.Modified, saved.Modified);
            Assert.IsTrue(saved.SubTypes.IsEmpty);
            Assert.AreEqual(item.Type, saved.Type);
            Assert.AreEqual(null, saved.TryGetBody<string>());
            Assert.AreEqual(0, saved.TryGetBody<int>());
            Assert.AreEqual(null, saved.TryGetBody<int?>());
        }


        [Test]
        public void SaveExisting_NotExisting()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            Assert.Throws<ItemNotFoundException>(() => provider.SaveExisting(item));
        }




        [Test]
        public void SaveNew()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

            var id = provider.SaveNew(item);
            Assert.AreEqual(id, item.Guid);
        }


        [Test]
        public void SaveNewAndGet()
        {
            var item = provider.NewItem(provider.Root, typeManager.GetTypeItem(DeviceType.Meter));

            item.Order = Int32.MaxValue;
            item.ValueDate = new DateTime(2100, 1, 1);
            item.ValueNumber = Math.PI;
            item.ValueString = new string('C', 1024 * 1024);

            var id = provider.SaveNew(item);

            var actual = provider.GetItemByReference(item);

            Assert.AreNotEqual(Guid.Empty, actual.Guid);
            Assert.AreEqual(id, actual.Guid);
            Assert.AreEqual(actual.Type, typeManager.GetTypeItem(DeviceType.Meter));
            Assert.AreEqual(actual.Children.Count, 0);
            Assert.AreEqual(actual.Related.Count, 0);
            Assert.AreEqual(actual.Created, item.Created);
            Assert.AreEqual(actual.Modified, item.Modified);
            Assert.IsFalse(actual.IsParentResolved);
            Assert.AreEqual(actual.Debug, false);
            Assert.AreEqual(actual.Name, "DeviceType");
            Assert.AreEqual(actual.Order, Int32.MaxValue);
            Assert.AreEqual(actual.Parent, provider.Root);
            Assert.AreEqual(actual.Revision, 0);
            Assert.AreEqual(actual.ValueString, item.ValueString);
            Assert.AreEqual(actual.ValueNumber, Math.PI);
            Assert.AreEqual(actual.ValueDate, item.ValueDate);
            Assert.IsTrue(actual.SubTypes.IsEmpty);
        }
    }
}