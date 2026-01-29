using PckTool.Core.WWise.Common;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class DataChunk : BaseChunk
{
    public override bool IsValid => Data is not null;

    public override uint Magic => Hash.AkmmioFourcc('D', 'A', 'T', 'A');

    public List<MediaIndexEntry>? Data { get; private set; }

    /// <summary>
    ///     Sets the data entries. Used for serialization when creating banks from scratch.
    /// </summary>
    /// <param name="entries">The media data entries to set.</param>
    internal void SetData(List<MediaIndexEntry> entries)
    {
        Data = entries;
    }

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

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        if (Data is null) return;

        // Write media data sequentially - offsets are calculated by SyncMediaChunks()
        foreach (var entry in Data)
        {
            writer.Write(entry.Data);
        }
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
