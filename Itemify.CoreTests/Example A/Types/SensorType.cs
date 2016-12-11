using Itemify.Core.Typing;

namespace Itemify.Spec.Example_A.Types
{
    [TypeDefinition("SensorType")]
    internal enum SensorType
    {
        [TypeValue("temp")]
        Temperature,

        [TypeValue("settemp")]
        SetTemperature,

        [TypeValue("bright")]
        Brightness
    }
}
