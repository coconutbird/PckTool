using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class PropBundle
{
    public bool IsRandomizer { get; set; }
    public bool IsModulator { get; set; }
    public List<Prop> Props { get; set; } = new();

    /// <summary>
    ///     Reads a property bundle from the binary reader.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <param name="isRandomizer">Whether this is a ranged modifier bundle (min+max values).</param>
    /// <param name="isModulator">
    ///     Whether this bundle belongs to a modulator (LFO/Envelope).
    ///     Modulators use AkModulatorPropID, not AkPropID, and all values are 4-byte floats.
    /// </param>
    public bool Read(BinaryReader reader, bool isRandomizer = false, bool isModulator = false)
    {
        IsRandomizer = isRandomizer;
        IsModulator = isModulator;
        var numberOfProps = reader.ReadByte();
        var ids = new byte[numberOfProps];

        for (var i = 0; i < numberOfProps; ++i)
        {
            ids[i] = reader.ReadByte();
        }

        for (var i = 0; i < numberOfProps; ++i)
        {
            var propId = (PropType) ids[i];
            var size = Prop.GetSizeOfType(propId, isRandomizer, isModulator);
            var propValue = reader.ReadBytes(size);
            var prop = new Prop { Id = propId, RawValue = propValue };

            Props.Add(prop);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Props.Count);

        // Write all IDs first
        foreach (var prop in Props)
        {
            writer.Write((byte) prop.Id);
        }

        // Then write all values
        foreach (var prop in Props)
        {
            writer.Write(prop.RawValue);
        }
    }
}
