using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class Prop
{
    public PropType Id { get; set; }
    public byte[] RawValue { get; set; } = [];

    /// <summary>
    ///     Gets the size in bytes for a property value.
    ///     Per wwiser's AkPropBundle_AkPropValue_unsigned_char___SetInitialParams:
    ///     ALL prop values in PropBundles are stored as 4-byte unions (AkPropValue),
    ///     regardless of their logical type (curves, probability, etc.).
    /// </summary>
    /// <param name="type">The property type ID (not used for sizing, kept for potential future use).</param>
    /// <param name="isRandomizer">Whether this is a ranged modifier (min+max values).</param>
    /// <param name="isModulator">
    ///     Whether this property belongs to a modulator (LFO/Envelope).
    ///     Modulators use AkModulatorPropID, not AkPropID, and all values are 4-byte floats.
    /// </param>
    /// <returns>The size in bytes for the property value.</returns>
    public static int GetSizeOfType(PropType type, bool isRandomizer = false, bool isModulator = false)
    {
        // Ranged modifiers always have 8 bytes (min + max, each as AkPropValue/union which is 4 bytes).
        // This applies to both regular objects and modulators.
        if (isRandomizer)
        {
            return 8;
        }

        // Per wwiser, ALL prop values in PropBundles are stored as 4-byte unions.
        // The prop ID is 1 byte, but the value is always a 4-byte union (AkPropValue).
        // This is true for all props including Probability, FadeCurve types, etc.
        return 4;
    }
}
