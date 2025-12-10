namespace SoundsUnpack.WWise;

public class StringMap
{
    public Dictionary<uint, string> Map { get; } = new();

    public bool Read(BinaryReader reader, uint size)
    {
        Map.Clear();

        var baseOffset = reader.BaseStream.Position;

        var numberOfStrings = reader.ReadUInt32();

        if (numberOfStrings == 0)
        {
            reader.BaseStream.Position = baseOffset + size;

            return true;
        }

        // Read all StringEntry structs first (offset + id pairs)
        var entries = new (uint Offset, uint Id)[numberOfStrings];

        for (var i = 0; i < numberOfStrings; ++i)
        {
            var offset = reader.ReadUInt32();
            var id = reader.ReadUInt32();

            if (offset > baseOffset + size)
            {
                return false;
            }

            entries[i] = (offset, id);
        }

        // Now read the strings at their offsets
        foreach (var (offset, id) in entries)
        {
            var stringOffset = baseOffset + offset;

            reader.BaseStream.Position = stringOffset;

            var str = reader.ReadWString();

            Map.TryAdd(id, str);
        }

        reader.BaseStream.Position = baseOffset + size;

        return true;
    }

    public void Write(BinaryWriter writer) { }
}
