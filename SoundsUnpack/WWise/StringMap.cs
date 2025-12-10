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

    /// <summary>
    ///     Writes the string map to a BinaryWriter and returns the total size written.
    /// </summary>
    public uint Write(BinaryWriter writer)
    {
        var startPosition = writer.BaseStream.Position;

        // Write count
        writer.Write((uint) Map.Count);

        if (Map.Count == 0)
        {
            return (uint) (writer.BaseStream.Position - startPosition);
        }

        // Calculate where strings will start (after all offset+id pairs)
        // Header: 4 bytes (count) + 8 bytes per entry (offset + id)
        var stringsStartOffset = 4 + Map.Count * 8;

        // First pass: calculate string offsets and write entry headers
        var stringOffsets = new List<(uint Id, uint Offset, string Value)>();
        var currentStringOffset = (uint) stringsStartOffset;

        foreach (var (id, value) in Map)
        {
            stringOffsets.Add((id, currentStringOffset, value));

            // Each char is 2 bytes (wide string) + 2 bytes for null terminator
            currentStringOffset += (uint) ((value.Length + 1) * 2);
        }

        // Write offset+id pairs
        foreach (var (id, offset, _) in stringOffsets)
        {
            writer.Write(offset);
            writer.Write(id);
        }

        // Write strings (wide strings, null-terminated)
        foreach (var (_, _, value) in stringOffsets)
        {
            foreach (var c in value)
            {
                writer.Write((ushort) c);
            }

            writer.Write((ushort) 0); // null terminator
        }

        return (uint) (writer.BaseStream.Position - startPosition);
    }
}
