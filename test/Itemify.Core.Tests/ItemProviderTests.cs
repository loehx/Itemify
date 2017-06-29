using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Itemify.Core.Exceptions;
using Itemify.Core.Item;
using Itemify.Core.ItemAccess;
using Itemify.Core.PostgreSql;
using Itemify.Logging;
using Itemify.Shared.Utils;
using Itemify.Spec.Example_A.Types;
using Xunit;

namespace Itemify.Core.Spec
{
    public class ItemProviderTests : IDisposable
    {
        private const string SCHEMA = "spec";
        private const string CONNECTION_STRING = "Server=127.0.0.1;Port=5432;Database=itemic;User Id=postgres;Password=abc;";
        private PostgreSqlConnectionPool connectionPool = new PostgreSqlConnectionPool(CONNECTION_STRING, 60, 5000);
        private PostgreSqlProvider sqlProvider;
        private EntityProvider entityProvider;
        private ItemProvider provider;
        private RegionBasedLogWriter logwriter;



        #region -----[   Test setup   ]------------------------------------------------------------------------------------------------------------------------------

        public ItemProviderTests()
        {
            var log = new CustomLogData(l => Debug.WriteLine(l));
            logwriter = new RegionBasedLogWriter(log, "Spec");
            sqlProvider = new PostgreSqlProvider(connectionPool, logwriter.NewRegion(nameof(PostgreSqlProvider)), SCHEMA);
            sqlProvider.EnsureSchemaExists();

            entityProvider = new EntityProvider(sqlProvider, logwriter.NewRegion(nameof(EntityProvider)));
            provider = new ItemProvider(entityProvider, logwriter.NewRegion(nameof(ItemProvider)));

            var tables = sqlProvider.GetTableNamesBySchema(SCHEMA);
            foreach (var table in tables)
            {
                sqlProvider.TryDropTable(table);
            }
        }

        public void Dispose()
        {
            sqlProvider.Dispose();
            logwriter.Dispose();
        }

        [Fact]
        public void RootItem()
        {
            Assert.NotNull(provider.Root);
            Assert.Equal(provider.Root.Guid, Guid.Empty);
            Assert.Equal(provider.Root.Type, "root");
        }


        [Fact]
        public void NewItem_A()
        {
            var item = new DefaultItem();

            Assert.NotNull(item);
            Assert.Equal(Guid.Empty, item.Guid);
            Assert.Equal(item.Type, DefaultTypes.Unknown);
            Assert.Equal(item.Children.Count, 0);
            Assert.Equal(item.Related.Count, 0);
            Assert.Equal(item.Created, DateTime.MinValue);
            Assert.Equal(item.Modified, DateTime.MinValue);
            Assert.False(item.IsParentResolved);
            Assert.Equal(item.Debug, false);
            Assert.Equal(item.Name, "[unknown]");
            Assert.Equal(item.Order, 0);
            Assert.Equal(item.Parent, provider.Root);
            Assert.Equal(item.Revision, 0);
            Assert.Equal(item.ValueString, null);
            Assert.Equal(item.ValueNumber, null);
            Assert.Equal(item.ValueDate, null);
            Assert.Equal(item.TryGetBody<object>(), null);
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


        #endregion

        #region -----[   Item instance   ]------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void NewItem_B()
        {
            var item = new DefaultItem();

            item.Guid = Guid.NewGuid();
            item.Name = "Example item";
            item.Order = int.MinValue;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = double.MinValue * 1.1;
            item.ValueString = new string('E', 1024 * 1024);

            Assert.NotEqual(Guid.Empty, item.Guid);
            Assert.Equal(item.Type, DefaultTypes.Unknown);
            Assert.Equal(item.Children.Count, 0);
            Assert.Equal(item.Related.Count, 0);
            Assert.Equal(item.Created, DateTime.MinValue);
            Assert.Equal(item.Modified, DateTime.MinValue);
            Assert.False(item.IsParentResolved);
            Assert.Equal(item.Debug, false);
            Assert.Equal(item.Name, "Example item");
            Assert.Equal(item.Order, int.MinValue);
            Assert.Equal(item.Parent, provider.Root);
            Assert.Equal(item.Revision, 0);
            Assert.Equal(item.ValueString.Length, 1024 * 1024);
            Assert.Equal(item.ValueString, new string('E', 1024 * 1024));
            Assert.Equal(item.ValueNumber, double.MinValue * 1.1);
            Assert.Equal(item.ValueDate, DateTime.MinValue.AddMilliseconds(1));
        }

        [Fact]
        public void NewItem_C()
        {
            var item = new DefaultItem();

            item.Name = "";
            item.Order = int.MaxValue;
            item.ValueDate = DateTime.MaxValue;
            item.ValueNumber = double.MaxValue;
            item.ValueString = "";

            Assert.Equal(Guid.Empty, item.Guid);
            Assert.Equal(item.Type, DefaultTypes.Unknown);
            Assert.Equal(item.Children.Count, 0);
            Assert.Equal(item.Related.Count, 0);
            Assert.Equal(item.Created, DateTime.MinValue);
            Assert.Equal(item.Modified, DateTime.MinValue);
            Assert.False(item.IsParentResolved);
            Assert.Equal(item.Debug, false);
            Assert.Equal(item.Name, "[unknown]");
            Assert.Equal(item.Order, int.MaxValue);
            Assert.Equal(item.Parent, provider.Root);
            Assert.Equal(item.Revision, 0);
            Assert.Equal(item.ValueString, null);
            Assert.Equal(item.ValueNumber, double.MaxValue);
            Assert.Equal(item.ValueDate, DateTime.MaxValue);
        }

        [Fact]
        public void NewItem_BodyA()
        {
            var item = new DefaultItem();

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
            Assert.Equal(actualBody.StringValue, body.StringValue);
            Assert.Equal(actualBody.BooleanValue, body.BooleanValue);
            Assert.Equal(actualBody.BinaryValue[0], body.BinaryValue[0]);
            Assert.Equal(actualBody.BinaryValue[1], body.BinaryValue[1]);
            Assert.Equal(actualBody.BinaryValue[2], body.BinaryValue[2]);
            Assert.Equal(actualBody.DateTimeValue, body.DateTimeValue);
            Assert.Equal(actualBody.DecimalValue, body.DecimalValue);
            Assert.Equal(actualBody.DoubleValue, body.DoubleValue);
            Assert.Equal(actualBody.IntValue, body.IntValue);
            Assert.Equal(actualBody.TimeSpanValue, body.TimeSpanValue);
            Assert.Equal(actualBody.Collection[0], body.Collection[0]);
            Assert.Equal(actualBody.Collection[1], body.Collection[1]);
            Assert.Equal(actualBody.Collection[2], body.Collection[2]);
            Assert.Equal(actualBody.Collection[3], body.Collection[3]);

            Assert.Equal(((ExampleBodyA.ComplexTypeA)actualBody.ComplexValue).StringValue, ((ExampleBodyA.ComplexTypeA)actualBody.ComplexValue).StringValue);
        }


        [Fact]
        public void NewItem_BadNumberValue()
        {
            var item = new DefaultItem();

            Assert.Throws<ArgumentOutOfRangeException>(() => item.ValueNumber = double.MinValue); // reserved by Itemify
        }

        [Fact]
        public void NewItem_BadDateValue()
        {
            var item = new DefaultItem();

            Assert.Throws<ArgumentOutOfRangeException>(() => item.ValueDate = DateTime.MinValue); // reserved by Itemify
        }

        //[Fact]
        //public void SerializeItem()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void DeserializeItem_FromString()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void DeserializeItem_FromBadString()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region -----[   Save and get item   ]------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void SaveItem()
        {
            var item = new DefaultItem(Guid.NewGuid());

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

            var id = provider.Save(item);
            Assert.Equal(id, item.Guid);
        }


        [Fact]
        public void SaveAndGetItem()
        {
            var item = new DefaultItem(Guid.NewGuid());

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

            var id = provider.Save(item);

            var actual = provider.GetItemByReference(item);

            Assert.NotEqual(Guid.Empty, actual.Guid);
            Assert.Equal(id, actual.Guid);
            //Assert.Equal(actual.Type, typeManager.GetTypeItem(DeviceType.Meter));
            Assert.Equal(actual.Children.Count, 0);
            Assert.Equal(actual.Related.Count, 0);
            Assert.Equal(actual.Created, item.Created);
            Assert.Equal(actual.Modified, item.Modified);
            Assert.False(actual.IsParentResolved);
            Assert.Equal(actual.Debug, false);
            Assert.Equal(actual.Name, "Example");
            Assert.Equal(actual.Order, -1);
            Assert.Equal(actual.Parent, provider.Root);
            Assert.Equal(actual.Revision, 0);
            Assert.Equal(actual.ValueString, "string");
            Assert.Equal(actual.ValueNumber, 1.1);
            Assert.Equal(actual.ValueDate, DateTime.MinValue.AddMilliseconds(1));
        }



        [Fact]
        public void SaveAndGetItem_SetValuesToNull()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Meter);

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

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

            Assert.Equal(id, final.Guid);
            Assert.Equal(1, final.Revision);
            Assert.Equal("New name", final.Name);
            Assert.Equal(5, final.Order);
            Assert.Equal(null, final.ValueDate);
            Assert.Equal(null, final.ValueNumber);
            Assert.Equal(null, final.ValueString);
            Assert.Equal(0, final.Children.Count);
            Assert.Equal(0, final.Related.Count);
            Assert.Equal(item.Created, final.Created);
            Assert.NotEqual(item.Modified, final.Modified);
            Assert.Equal(item.Type, final.Type);
            Assert.Equal(null, final.GetBody<string>());
        }



        [Fact]
        public void SaveAndGetItem_MinimumInformation()
        {
            var item = new DefaultItem(Guid.NewGuid());
            var id = provider.Save(item);
            var saved = provider.GetItemByReference(item);

            Assert.Equal(id, saved.Guid);
            Assert.Equal(0, saved.Revision);
            Assert.Equal("[unknown]", saved.Name);
            Assert.Equal(0, saved.Order);
            Assert.Equal(null, saved.ValueDate);
            Assert.Equal(null, saved.ValueNumber);
            Assert.Equal(null, saved.ValueString);
            Assert.Equal(0, saved.Children.Count);
            Assert.Equal(0, saved.Related.Count);
            Assert.Equal(item.Created, saved.Created);
            Assert.Equal(item.Modified, saved.Modified);
            Assert.Equal(item.Type, saved.Type);
            Assert.Equal(null, saved.TryGetBody<string>());
            Assert.Equal(0, saved.TryGetBody<int>());
            Assert.Equal(null, saved.TryGetBody<int?>());
        }

        [Fact]
        public void SaveExisting_NotExisting()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Actor);

            Assert.Throws<ItemNotFoundException>(() => provider.SaveExisting(item));
        }

        [Fact]
        public void SaveNew()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Actor);

            item.Name = "Example";
            item.Order = -1;
            item.ValueDate = DateTime.MinValue.AddMilliseconds(1);
            item.ValueNumber = 1.1;
            item.ValueString = "string";

            var id = provider.SaveNew(item);
            Assert.Equal(id, item.Guid);
        }


        [Fact]
        public void SaveNewAndGet()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Actor);

            item.Order = Int32.MaxValue;
            item.ValueDate = new DateTime(2100, 1, 1);
            item.ValueNumber = Math.PI;
            item.ValueString = new string('C', 1024 * 1024);

            var id = provider.SaveNew(item);

            var actual = provider.GetItemByReference(item);

            Assert.NotEqual(Guid.Empty, actual.Guid);
            Assert.Equal(id, actual.Guid);
            //Assert.Equal(actual.Type, typeManager.GetTypeItem(DeviceType.Meter));
            Assert.Equal(actual.Children.Count, 0);
            Assert.Equal(actual.Related.Count, 0);
            Assert.Equal(actual.Created, item.Created);
            Assert.Equal(actual.Modified, item.Modified);
            Assert.False(actual.IsParentResolved);
            Assert.Equal(actual.Debug, false);
            Assert.Equal(actual.Name, "[Actor]");
            Assert.Equal(actual.Order, Int32.MaxValue);
            Assert.Equal(actual.Parent, provider.Root);
            Assert.Equal(actual.Revision, 0);
            Assert.Equal(actual.ValueString, item.ValueString);
            Assert.Equal(actual.ValueNumber, Math.PI);
            Assert.Equal(actual.ValueDate, item.ValueDate);
        }


        #endregion
        
        #region -----[   Remove item   ]------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RemoveItem()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Actor)
            {
                ValueDate = DateTime.MinValue.AddMilliseconds(1)
            };

            var id = provider.Save(item);
            Assert.Equal(id, item.Guid);

            var found = provider.GetItemByReference(item);
            Assert.NotNull(found);

            provider.RemoveItemByReference(item);

            found = provider.GetItemByReference(item);
            Assert.Null(found);
        }
      
        #endregion

        #region -----[   Query item   ]------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetItemsByStringValue()
        {
            var item = new DefaultItem(Guid.NewGuid());

            item.Type = DeviceType.Actor;
            item.Name = "Example";
            item.ValueString = "test string";

            var id = provider.Save(item);
            Assert.Equal(id, item.Guid);

            var actual = provider.GetItemsByStringValue("test string", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByStringValue("test%", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByStringValue("%st strin%", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByStringValue("TEST st%", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByStringValue("TEST%ng", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByStringValue("test_string", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(0, actual.Length);

            actual = provider.GetItemsByStringValue("TEST_str___", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(0, actual.Length);

            actual = provider.GetItemsByStringValue("test1%", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(0, actual.Length);

            actual = provider.GetItemsByStringValue("test", item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(0, actual.Length);
        }

        [Fact]
        public void GetItemsByNumberValue()
        {
            var item = new DefaultItem(Guid.NewGuid());

            item.Type = DeviceType.Actor;
            item.Name = "Example";
            item.ValueNumber = 5;

            var id = provider.Save(item);
            Assert.Equal(id, item.Guid);

            var actual = provider.GetItemsByNumberValue(1, 10, item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByNumberValue(1, 5, item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByNumberValue(5, 10, item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByNumberValue(6, 10, item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(0, actual.Length);
        }

        [Fact]
        public void GetItemsByDateTimeValue()
        {
            var item = new DefaultItem(Guid.NewGuid());

            item.Type = DeviceType.Actor;
            item.Name = "Example";
            item.ValueDate = new DateTime(2017, 1, 5);

            var id = provider.Save(item);
            Assert.Equal(id, item.Guid);

            var actual = provider.GetItemsByDateTimeValue(new DateTime(2017, 1, 1), new DateTime(2017, 1, 10), item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByDateTimeValue(new DateTime(2017, 1, 5), new DateTime(2017, 1, 10), item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByDateTimeValue(new DateTime(2017, 1, 1), new DateTime(2017, 1, 5), item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(1, actual.Length);
            Assert.Equal(id, actual[0].Guid);

            actual = provider.GetItemsByDateTimeValue(new DateTime(2017, 1, 6), new DateTime(2017, 1, 10), item.Type, ItemResolving.Default).ToArray();
            Assert.Equal(0, actual.Length);
        }

        #endregion

        #region -----[   Relations   ]------------------------------------------------------------------------------------------------------------------------------


        [Fact]
        public void SetRelation_ToTwoItems()
        {
            var itemA = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);
            var itemB = new DefaultItem(Guid.NewGuid(), SensorType.Temperature);
            var itemC = new DefaultItem(Guid.NewGuid(), SensorType.SetTemperature);

            provider.SaveNew(itemA);
            provider.SaveNew(itemB);
            provider.SaveNew(itemC);

            provider.SetRelations(itemA, itemB, itemC);

            var actual = provider.GetItemByReference(itemA, ItemResolving.Default.RelatedItemsOfType("test", SensorType.Temperature));

            Assert.Equal(1, actual.Related.Count);
            Assert.Equal(itemB.Guid, actual.Related.First().Guid);
            Assert.Equal(itemB.Type, actual.Related.First().Type);

            actual = provider.GetItemByReference(itemA, ItemResolving.Default.RelatedItemsOfType(SensorType.SetTemperature));

            Assert.Equal(1, actual.Related.Count);
            Assert.Equal(itemC.Guid, actual.Related.First().Guid);
            Assert.Equal(itemC.Type, actual.Related.First().Type);

            actual = provider.GetItemByReference(itemA, ItemResolving.Default.RelatedItemsOfType(SensorType.Temperature, SensorType.SetTemperature));
            Assert.Equal(2, actual.Related.Count);
        }

        [Fact]
        public void SetRelation_Bidirectional()
        {
            var itemA = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);
            var itemB = new DefaultItem(Guid.NewGuid(), SensorType.Temperature);
            var itemC = new DefaultItem(Guid.NewGuid(), SensorType.SetTemperature);

            provider.SaveNew(itemA);
            provider.SaveNew(itemB);
            provider.SaveNew(itemC);

            provider.SetRelations(itemA, itemB, itemC);

            var actual = provider.GetItemByReference(itemB, ItemResolving.Default.RelatedItemsOfType(DeviceType.Sensor));

            Assert.Equal(1, actual.Related.Count);
            Assert.Equal(itemA.Guid, actual.Related.First().Guid);
            Assert.Equal(itemA.Type, actual.Related.First().Type);
        }

        [Fact]
        public void RemoveRelation()
        {
            var itemA = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);
            var itemB = new DefaultItem(Guid.NewGuid(), SensorType.Temperature);

            provider.SaveNew(itemA);
            provider.SaveNew(itemB);

            provider.SetRelations(itemA, itemB);

            var actual = provider.GetItemByReference(itemA, ItemResolving.Default.RelatedItemsOfType(SensorType.Temperature));
            Assert.Equal(1, actual.Related.Count);

            provider.RemoveRelations(itemA, SensorType.Brightness);

            actual = provider.GetItemByReference(itemA, ItemResolving.Default.RelatedItemsOfType(SensorType.Temperature));
            Assert.Equal(1, actual.Related.Count);

            provider.RemoveRelations(itemA, SensorType.Temperature);

            actual = provider.GetItemByReference(itemA, ItemResolving.Default.RelatedItemsOfType(SensorType.Temperature));
            Assert.Equal(0, actual.Related.Count);
        }


        //[Fact]
        //public void SetRelation_ToSeveralItems()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SetRelation_ToSingleItemTwice()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SetRelation_ToSeveralItemsTwice()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        ////[ExpectedException(typeof(ItemNotFoundException))]
        //public void SetRelation_ToNotExistingItem()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        ////[ExpectedException(typeof(Exception))]
        //public void SetRelation_ToSameItem()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        ////[ExpectedException(typeof(Exception))]
        //public void SetRelation_ToRootItem()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void RemoveRelation_ToSingleItem()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void RemoveRelation_ToSeveralItems()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void RemoveRelation_ToSingleItemTwice()
        //{
        //    throw new NotImplementedException();
        //}


        //[Fact]
        //public void SaveAndGetNewItem_WithNewRelation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetNewItem_WithExistingRelation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetNewItem_WithNotExistingRelation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetNewItem_WithNewRelations()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetNewItem_WithExistingRelations()
        //{
        //    throw new NotImplementedException();
        //}


        //[Fact]
        //public void SaveAndGetNewItem_WithExistingAndNewRelations()
        //{
        //    throw new NotImplementedException();
        //}


        //[Fact]
        //public void SaveAndGetExistingItem_WithNewRelation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithExistingRelation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithNotExistingRelation()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithNewRelations()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithExistingRelations()
        //{
        //    throw new NotImplementedException();
        //}


        //[Fact]
        //public void SaveAndGetExistingItem_WithExistingAndNewRelations()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        #region -----[   Children   ]------------------------------------------------------------------------------------------------------------------------------




        [Fact]
        public void SaveAndGetNewItem_WithNewChild()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);
            var child = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);

            provider.SaveNew(item);
            provider.SaveNew(child);

            var actual = provider.GetItemByReference(item, ItemResolving.Default.ChildrenOfType(SensorType.Temperature));

            Assert.Equal(1, actual.Children.Count);
            Assert.Equal(child.Guid, actual.Children.First().Guid);
            Assert.Equal(child.Type, actual.Children.First().Type);

            var rootChildren = provider.GetChildrenOfItemByReference(provider.Root, SensorType.Temperature, DeviceType.Sensor).ToArray();
            Assert.Equal(1, rootChildren.Length);
            Assert.Equal(1, rootChildren.Count(k => k.Type.Equals(DeviceType.Sensor)));
        }

        [Fact]
        public void SaveAndGetNewItem_WithExistingChild()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);
            var child = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);

            provider.SaveNew(item);
            provider.SaveNew(child);

            var actual = provider.GetItemByReference(item, ItemResolving.Default.ChildrenOfType(SensorType.Temperature));

            Assert.Equal(1, actual.Children.Count);
            Assert.Equal(child.Guid, actual.Children.First().Guid);
            Assert.Equal(child.Type, actual.Children.First().Type);
        }

        [Fact]
        public void SaveAndGetNewItem_WithExistingChild_MissingId()
        {
            var item = new DefaultItem(DeviceType.Sensor);

            Assert.Throws<Exception>(() =>
            {
                provider.SaveNew(item);
            });
        }


        [Fact]
        public void SaveAndGetNewItem_WithNewChildren()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);

            var childA = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);
            var childB = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);
            var childC = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);

            provider.SaveNew(item);
            new[] { childA, childB, childC }.ForEach(k => provider.SaveNew(k));

            var actual = provider.GetItemByReference(item, ItemResolving.Default.ChildrenOfType(SensorType.Temperature));

            Assert.Equal(3, actual.Children.Count);
            Assert.Equal(childA.Guid, actual.Children.Skip(0).First().Guid);
            Assert.Equal(childA.Type, actual.Children.Skip(0).First().Type);

            Assert.Equal(childB.Guid, actual.Children.Skip(1).First().Guid);
            Assert.Equal(childB.Type, actual.Children.Skip(1).First().Type);

            Assert.Equal(childC.Guid, actual.Children.Skip(2).First().Guid);
            Assert.Equal(childC.Type, actual.Children.Skip(2).First().Type);
        }


        [Fact]
        public void SaveAndGetNewItem_WithNewChildren_Implicit()
        {
            var item = new DefaultItem(Guid.NewGuid(), DeviceType.Sensor);
            provider.SaveNew(item);

            var childA = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);
            var childB = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);
            var childC = new DefaultItem(Guid.NewGuid(), SensorType.Temperature, item);

            new[] { childA, childB, childC }.ForEach(k => provider.SaveNew(k));

            var actual = provider.GetItemByReference(item, ItemResolving.Default.ChildrenOfType(SensorType.Temperature));

            Assert.Equal(3, actual.Children.Count);
            Assert.Equal(childA.Guid, actual.Children.Skip(0).First().Guid);
            Assert.Equal(childA.Type, actual.Children.Skip(0).First().Type);

            Assert.Equal(childB.Guid, actual.Children.Skip(1).First().Guid);
            Assert.Equal(childB.Type, actual.Children.Skip(1).First().Type);

            Assert.Equal(childC.Guid, actual.Children.Skip(2).First().Guid);
            Assert.Equal(childC.Type, actual.Children.Skip(2).First().Type);
        }



        //[Fact]
        //public void SaveAndGetExistingItem_WithNewChild()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithExistingChild()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithNotExistingChild()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithNewChildren()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public void SaveAndGetExistingItem_WithExistingChildren()
        //{
        //    throw new NotImplementedException();
        //}


        //[Fact]
        //public void SaveAndGetExistingItem_WithExistingAndNewChildren()
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

    }
}