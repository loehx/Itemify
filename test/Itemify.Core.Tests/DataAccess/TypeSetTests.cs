using System;
using Itemify.Core.Typing;
using Itemify.Spec.Example_A.Types;
using NUnit.Framework;

namespace Itemify.Spec
{
    [TestFixture]
    public class TypeSetTests
    {
        private TypeManager typeManager;

        [SetUp]
        public void BeforeEach()
        {
            typeManager = new TypeManager();
            typeManager.Register<DeviceType>();
        }

        [Test]
        public void TypeSetTest()
        {
            var a = typeManager.GetTypeSet(DeviceType.Sensor);
            var b = typeManager.GetTypeSet(DeviceType.Meter);

            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void TypeSetFromUnregisteredEnum()
        {
            Assert.Throws<Exception>(() => typeManager.GetTypeSet(DeviceTypesDuplicateTypeValue.Sensor));
        }

        [Test]
        public void TypeSetSerialization()
        {
            var set = typeManager.GetTypeSet(DeviceType.Sensor);
            var raw = set.ToStringValue();

            var actual = typeManager.ParseTypeSet(raw);
            Assert.AreEqual(set, actual);
        }

    }
}
