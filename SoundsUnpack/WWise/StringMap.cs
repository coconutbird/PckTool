namespace SoundsUnpack.WWise;

public class StringMap
{
    public bool Read(BinaryReader reader, uint size)
    {
        Map.Clear();

        var baseOffset = reader.BaseStream.Position;

        var numberOfStrings = reader.ReadUInt32();

        for (var i = 0; i < numberOfStrings; ++i)
        {
            var offset = reader.ReadUInt32();
            var id = reader.ReadUInt32();

            var lastPos = reader.BaseStream.Position;

            var stringOffset = baseOffset + offset;

            if (stringOffset >= baseOffset + size)
            {
                break;
            }

            reader.BaseStream.Position = stringOffset;

            var str = reader.ReadWString();

            Map.Add(id, str);

            reader.BaseStream.Position = lastPos;
        }

        reader.BaseStream.Position = baseOffset + size;

        return true;
    }

    public void Write(BinaryWriter writer) { }

    public Dictionary<uint, string> Map { get; } = new();
}