using System;
using System.Data;
using Itemify.Core.Exceptions;
using Itemify.Core.Typing;
using Itemify.Spec.Example_A.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Itemify.Spec
{
    [TestClass]
    public class RegisterTests
    {
        [TestMethod]
        public void DeviceTypes()
        {
            new TypeManager().Register<DeviceType>();
        }

        [TestMethod]
        [ExpectedException(typeof(MissingCustomAttribute))]
        public void DeviceTypesMissingDefinitionAttribute()
        {
            new TypeManager().Register<DeviceTypesMissingDefinitionAttribute>();
        }

        [TestMethod]
        [ExpectedException(typeof(MissingCustomAttribute))]
        public void DeviceTypesMissingSingleTypeValueAttribute()
        {
            new TypeManager().Register<DeviceTypesMissingSingleTypeValueAttribute>();
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateNameException))]
        public void DeviceTypesDuplicateTypeValue()
        {
            new TypeManager().Register<DeviceTypesDuplicateTypeValue>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PassingANonEnumToRegister()
        {
            new TypeManager().Register<NoEnum>();
        }

        private struct NoEnum
        {
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void RegisterSameDefinitionTwice()
        {
            var tm = new TypeManager();
            tm.Register<DeviceType>();
            tm.Register<DeviceType>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeviceTypesContainingIllegalCharacters()
        {
            new TypeManager().Register<DeviceTypesContainingIllegalCharacters>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeviceTypesContainingIllegalCharacters2()
        {
            new TypeManager().Register<DeviceTypesContainingIllegalCharacters2>();
        }
    }
}
