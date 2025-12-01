namespace SoundsUnpack.WWise.Structs;

public class PropBundle
{
    public bool Read(BinaryReader reader)
    {
        var numberOfProps = reader.ReadByte();
        for (var i = 0; i < numberOfProps; ++i)
        {
            var propId = reader.ReadUInt16();
            var propValue = reader.ReadUInt32();

            Props.Add(propId, propValue);
        }
    }
}