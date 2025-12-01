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
            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

            var buffer = reader.ReadBytes((int)entry.Size);

            Data.Add(new MediaIndexEntry
            {
                Id = entry.Id,
                Data = buffer
            });
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