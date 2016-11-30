using System;
using Itemify.Core.Typing;
using Itemify.Spec.Example_A.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Itemify.Spec
{
    [TestClass]
    public class TypeSetTests
    {
        [TestInitialize]
        public void BeforeEach()
        {
            TypeManager.Reset();
            TypeManager.Register<DeviceType>();
        }

        [TestMethod]
        public void TypeSetTest()
        {
            var a = TypeSet.From(DeviceType.Sensor);
            var b = TypeSet.From(DeviceType.Meter);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TypeSetFromUnregisteredEnum()
        {
            TypeSet.From(DeviceTypesDuplicateTypeValue.Sensor);
        }

        [TestMethod]
        public void TypeSetSerialization()
        {
            var set = TypeSet.From(DeviceType.Sensor);
            var raw = set.ToStringValue();

            var actual = TypeSet.Parse(raw);
            Assert.AreEqual(set, actual);
        }

    }
}
