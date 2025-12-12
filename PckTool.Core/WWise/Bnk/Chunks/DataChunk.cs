using PckTool.Core.WWise.Pck;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class DataChunk : BaseChunk
{
    public override bool IsValid => Data is not null;

    public List<MediaIndexEntry>? Data { get; private set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var mediaIndexChunk = soundBank.MediaIndexChunk;

        if (mediaIndexChunk?.LoadedMedia is null)
        {
            Log.Error("DataChunk requires MediaIndexChunk to be loaded first");

            return false;
        }

        var data = new List<MediaIndexEntry>();

        foreach (var header in mediaIndexChunk.LoadedMedia)
        {
            reader.BaseStream.Seek(startPosition + header.Offset, SeekOrigin.Begin);

            var buffer = reader.ReadBytes((int) header.Size);

            var entry = new MediaIndexEntry { Id = header.Id, Data = buffer };

            data.Add(entry);
        }

        // Save data
        Data = data;

        // Ensure the reader is positioned at the end of the chunk
        reader.BaseStream.Position = startPosition + size;

        return true;
    }

    public class MediaIndexEntry
    {
        private const uint ValidMagic = 0x46464952; // 'RIFF'
        public uint Id { get; set; }
        public byte[] Data { get; set; } = [];

        public bool IsValid => Data.Length >= 4 && BitConverter.ToUInt32(Data, 0) == ValidMagic;

        public override string ToString()
        {
            return $"MediaIndexEntry(Id={Id:X8}, DataLength={Data.Length})";
        }
    }
}
