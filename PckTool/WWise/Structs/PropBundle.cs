using PckTool.WWise.Enums;

namespace PckTool.WWise.Structs;

public class PropBundle
{
    public List<Prop> Props { get; set; } = new();

    public bool Read(BinaryReader reader, bool isRandomizer = false)
    {
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
}
