using System;
using System.Data;
using Itemify.Core.Exceptions;
using Itemify.Core.Typing;
using Itemify.Spec.Example_A.Types;
using NUnit.Framework;

namespace Itemify.Spec
{
    [TestFixture]
    public class RegisterTests
    {
        [Test]
        public void DeviceTypes()
        {
            new TypeManager().Register<DeviceType>();
        }

        [Test]
        public void DeviceTypesMissingDefinitionAttribute()
        {
            Assert.Throws<MissingCustomAttribute>(
                () => new TypeManager().Register<DeviceTypesMissingDefinitionAttribute>());
        }

        [Test]
        public void DeviceTypesMissingSingleTypeValueAttribute()
        {
            Assert.Throws<MissingCustomAttribute>(
                () => new TypeManager().Register<DeviceTypesMissingSingleTypeValueAttribute>());
        }

        [Test]
        public void DeviceTypesDuplicateTypeValue()
        {
            Assert.Throws<Exception>(
                () => new TypeManager().Register<DeviceTypesDuplicateTypeValue>());
        }

        [Test]
        public void PassingANonEnumToRegister()
        {
            Assert.Throws<ArgumentException>(
                () => new TypeManager().Register<NoEnum>());
        }

        private struct NoEnum
        {
        }

        [Test]
        public void RegisterSameDefinitionTwice()
        {
            Assert.Throws<Exception>(() =>
                {
                    var tm = new TypeManager();
                    tm.Register<DeviceType>();
                    tm.Register<DeviceType>();
                });
        }

        [Test]
        public void DeviceTypesContainingIllegalCharacters()
        {
            Assert.Throws<ArgumentException>(
                () => new TypeManager().Register<DeviceTypesContainingIllegalCharacters>());
        }

        [Test]
        public void DeviceTypesContainingIllegalCharacters2()
        {
            Assert.Throws<ArgumentException>(
                () => new TypeManager().Register<DeviceTypesContainingIllegalCharacters2>());
        }
    }
}
