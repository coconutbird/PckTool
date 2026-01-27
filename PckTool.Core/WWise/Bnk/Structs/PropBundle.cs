using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

public class PropBundle
{
    public bool IsRandomizer { get; set; }
    public List<Prop> Props { get; set; } = new();

    public bool Read(BinaryReader reader, bool isRandomizer = false)
    {
        IsRandomizer = isRandomizer;
        var numberOfProps = reader.ReadByte();
        var ids = new byte[numberOfProps];

        for (var i = 0; i < numberOfProps; ++i)
        {
            ids[i] = reader.ReadByte();
        }

        for (var i = 0; i < numberOfProps; ++i)
        {
            var propId = (PropType) ids[i];
            var propValue = reader.ReadBytes(Prop.GetSizeOfType(propId, isRandomizer));
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
