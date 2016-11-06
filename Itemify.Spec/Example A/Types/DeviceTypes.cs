using Itemify.Typing;

namespace Itemify.Spec.Example_A.Types
{
    [TypeDefinition("DeviceType")]
    internal enum DeviceType
    {
        [TypeValue("sensor")]
        Sensor,

        [TypeValue("meter")]
        Meter,

        [TypeValue("actor")]
        Actor
    }

    // <-- bad
    internal enum DeviceTypesMissingDefinitionAttribute
    {
        [TypeValue("sensor")]
        Sensor,

        [TypeValue("meter")]
        Meter,

        [TypeValue("actor")]
        Actor
    }

    [TypeDefinition("DeviceType")]
    internal enum DeviceTypesMissingSingleTypeValueAttribute
    {
        [TypeValue("sensor")]
        Sensor,

        [TypeValue("meter")]
        Meter,

        Actor // <-- bad
    }

    [TypeDefinition("DeviceType")]
    internal enum DeviceTypesDuplicateTypeValue
    {
        [TypeValue("sensor")]
        Sensor,

        [TypeValue("SENSOR")] // <-- bad
        Meter,

        [TypeValue("actor")]
        Actor
    }

    [TypeDefinition("Device-Type")] // <-- bad
    internal enum DeviceTypesContainingIllegalCharacters
    {
        [TypeValue("sensor")]
        Sensor
    }

    [TypeDefinition("DeviceType")]
    internal enum DeviceTypesContainingIllegalCharacters2
    {
        [TypeValue("sensor-item")] // <-- bad
        Sensor
    }
}
