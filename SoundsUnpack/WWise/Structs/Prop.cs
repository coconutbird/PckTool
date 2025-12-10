using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Structs;

public class Prop
{
    public PropType Id { get; set; }
    public byte[] RawValue { get; set; } = [];

    public static int GetSizeOfType(PropType type, bool isRandomizer = false)
    {
        return type switch
        {
            // Special cases
            PropType.Pitch => isRandomizer ? 8 : 4,          // double / 64-bit
            PropType.InitialDelay => isRandomizer ? 8 : 4,   // double / 64-bit
            PropType.TransitionTime => isRandomizer ? 8 : 4, // double / 64-bit
            PropType.LowPassFilter => isRandomizer ? 8 : 4,  // double / 64-bit
            PropType.Volume => isRandomizer ? 8 : 4,         // double / 64-bit
            PropType.Probability => sizeof(byte),            // stored as % (0-100)
            PropType.FadeInCurve => sizeof(byte),            // curve enum
            PropType.FadeOutCurve => sizeof(byte),
            PropType.CrossfadeUpCurve => sizeof(byte),
            PropType.CrossfadeDownCurve => sizeof(byte),

            // Everything else is a float (AkReal32)
            _ => sizeof(float)
        };
    }
}
