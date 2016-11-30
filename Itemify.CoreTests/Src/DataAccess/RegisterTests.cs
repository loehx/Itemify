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
        [TestInitialize]
        public void BeforeEach()
        {
            TypeManager.Reset();
        }

        [TestMethod]
        public void DeviceTypes()
        {
            TypeManager.Register<DeviceType>();
        }

        [TestMethod]
        [ExpectedException(typeof(MissingCustomAttribute))]
        public void DeviceTypesMissingDefinitionAttribute()
        {
            TypeManager.Register<DeviceTypesMissingDefinitionAttribute>();
        }

        [TestMethod]
        [ExpectedException(typeof(MissingCustomAttribute))]
        public void DeviceTypesMissingSingleTypeValueAttribute()
        {
            TypeManager.Register<DeviceTypesMissingSingleTypeValueAttribute>();
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateNameException))]
        public void DeviceTypesDuplicateTypeValue()
        {
            TypeManager.Register<DeviceTypesDuplicateTypeValue>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PassingANonEnumToRegister()
        {
            TypeManager.Register<NoEnum>();
        }

        private struct NoEnum
        {
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void RegisterSameDefinitionTwice()
        {
            TypeManager.Register<DeviceType>();
            TypeManager.Register<DeviceType>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeviceTypesContainingIllegalCharacters()
        {
            TypeManager.Register<DeviceTypesContainingIllegalCharacters>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DeviceTypesContainingIllegalCharacters2()
        {
            TypeManager.Register<DeviceTypesContainingIllegalCharacters2>();
        }
    }
}
