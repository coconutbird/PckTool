using PckTool.Core.WWise.Util;

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

        var mediaIndexChunk = soundBank.MediaIndexChunk;

        if (mediaIndexChunk?.LoadedMedia is null)
        {
            // No index chunk - write all media data sequentially (fallback)
            foreach (var entry in Data)
            {
                writer.Write(entry.Data);
            }

            return;
        }

        // Write media data at original offsets to preserve alignment/padding
        // The MediaHeader contains the original offset where each entry should be written
        var startPosition = writer.BaseStream.Position;

        for (var i = 0; i < Data.Count && i < mediaIndexChunk.LoadedMedia.Count; i++)
        {
            var header = mediaIndexChunk.LoadedMedia[i];
            var entry = Data[i];

            // Calculate how much padding we need before this entry
            var targetPosition = startPosition + header.Offset;
            var currentPosition = writer.BaseStream.Position;
            var paddingNeeded = (int) (targetPosition - currentPosition);

            // Write padding bytes (zeros) if needed
            if (paddingNeeded > 0)
            {
                writer.Write(new byte[paddingNeeded]);
            }
            else if (paddingNeeded < 0)
            {
                // This shouldn't happen if offsets are in order, but handle it gracefully
                Log.Warn(
                    "Media entry {0} offset overlap: expected position {1}, current position {2}",
                    header.Id,
                    targetPosition,
                    currentPosition);
            }

            // Write the media data
            writer.Write(entry.Data);

            // Update the header size in case the data changed (for modified banks)
            header.Size = (uint) entry.Data.Length;
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
