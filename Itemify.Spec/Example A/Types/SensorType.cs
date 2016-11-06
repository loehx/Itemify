using Itemify.Typing;

namespace Itemify.Spec.Example_A.Types
{
    [TypeDefinition("SensorType")]
    internal enum SensorType
    {
        [TypeValue("temp")]
        Temperature,

        [TypeValue("set-temp")]
        SetTemperature,

        [TypeValue("bright")]
        Brightness
    }
}
