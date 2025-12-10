namespace SoundsUnpack.WWise.Chunks;

public class DataChunk
{
    public List<MediaIndexEntry> Data { get; } = [];

    public bool Read(BinaryReader reader, MediaIndexChunk mediaIndexChunk, uint size)
    {
        Data.Clear();

        var baseOffset = reader.BaseStream.Position;

        foreach (var entry in mediaIndexChunk.LoadedMedia)
        {
            reader.BaseStream.Seek(baseOffset + entry.Offset, SeekOrigin.Begin);

            var buffer = reader.ReadBytes((int) entry.Size);

            var magic = BitConverter.ToUInt32(buffer, 0);

            if (magic != 0x46464952 && magic != 0x464D4557) // 'RIFF' or 'WEMF'
            {
                Console.WriteLine($"Warning: Media entry {entry.Id:X8} does not start with RIFF header.");
            }

            Data.Add(new MediaIndexEntry { Id = entry.Id, Data = buffer });
        }

        // Ensure the reader is positioned at the end of the chunk
        reader.BaseStream.Position = baseOffset + size;

        return true;
    }

    public class MediaIndexEntry
    {
        public uint Id { get; set; }
        public byte[] Data { get; set; } = [];
    }
}