using System;
using Itemify.Core.Typing;
using Itemify.Spec.Example_A.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Itemify.Spec
{
    [TestClass]
    public class TypeSetTests
    {
        private TypeManager typeManager;

        [TestInitialize]
        public void BeforeEach()
        {
            typeManager = new TypeManager();
            typeManager.Register<DeviceType>();
        }

        [TestMethod]
        public void TypeSetTest()
        {
            var a = typeManager.GetTypeSet(DeviceType.Sensor);
            var b = typeManager.GetTypeSet(DeviceType.Meter);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TypeSetFromUnregisteredEnum()
        {
            typeManager.GetTypeSet(DeviceTypesDuplicateTypeValue.Sensor);
        }

        [TestMethod]
        public void TypeSetSerialization()
        {
            var set = typeManager.GetTypeSet(DeviceType.Sensor);
            var raw = set.ToStringValue();

            var actual = typeManager.ParseTypeSet(raw);
            Assert.AreEqual(set, actual);
        }

    }
}
