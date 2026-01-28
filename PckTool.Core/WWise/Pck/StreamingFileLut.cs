using PckTool.Abstractions;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Lookup table for streaming file entries (uses 32-bit file IDs).
/// </summary>
public class StreamingFileLut : FileLut<uint, StreamingFileEntry>, IStreamingFileCollection
{
    protected override int KeySize => 4;

    /// <inheritdoc />
    IStreamingFileEntry? IStreamingFileCollection.this[uint sourceId] => this[sourceId];

    /// <inheritdoc />
    IEnumerable<uint> IStreamingFileCollection.SourceIds => ById.Keys;

    protected override uint ReadKey(BinaryReader reader)
    {
        return reader.ReadUInt32();
    }

    protected override void WriteKey(BinaryWriter writer, uint key)
    {
        writer.Write(key);
    }

    protected override StreamingFileEntry CreateEntry(uint id, uint blockSize, uint startBlock, uint languageId)
    {
        return new StreamingFileEntry
        {
            Id = id, BlockSize = blockSize, StartBlock = startBlock, LanguageId = languageId
        };
    }

    /// <inheritdoc />
    bool IStreamingFileCollection.TryGet(uint sourceId, out IStreamingFileEntry? entry)
    {
        var result = ById.TryGetValue(sourceId, out var streamingEntry);
        entry = streamingEntry;

        return result;
    }

    /// <inheritdoc />
    IEnumerator<IStreamingFileEntry> IEnumerable<IStreamingFileEntry>.GetEnumerator()
    {
        foreach (var entry in Entries)
        {
            yield return entry;
        }
    }
}
