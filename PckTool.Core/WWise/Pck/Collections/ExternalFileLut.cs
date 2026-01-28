using PckTool.Abstractions;
using PckTool.Core.WWise.Pck.Entries;

namespace PckTool.Core.WWise.Pck.Collections;

/// <summary>
///     Lookup table for external file entries (uses 64-bit file IDs).
/// </summary>
public class ExternalFileLut : FileLut<ulong, ExternalFileEntry>, IExternalFileCollection
{
    protected override int KeySize => 8;

    /// <inheritdoc />
    IExternalFileEntry? IExternalFileCollection.this[ulong fileId] => this[fileId];

    /// <inheritdoc />
    IReadOnlyList<IExternalFileEntry> IExternalFileCollection.Entries => Entries.Cast<IExternalFileEntry>().ToList();

    /// <inheritdoc />
    IEnumerable<ulong> IExternalFileCollection.FileIds => ById.Keys;

    /// <inheritdoc />
    bool IExternalFileCollection.TryGet(ulong fileId, out IExternalFileEntry? entry)
    {
        var result = ById.TryGetValue(fileId, out var externalEntry);
        entry = externalEntry;

        return result;
    }

    /// <inheritdoc />
    IEnumerator<IExternalFileEntry> IEnumerable<IExternalFileEntry>.GetEnumerator()
    {
        foreach (var entry in Entries)
        {
            yield return entry;
        }
    }

    protected override ulong ReadKey(BinaryReader reader)
    {
        return reader.ReadUInt64();
    }

    protected override void WriteKey(BinaryWriter writer, ulong key)
    {
        writer.Write(key);
    }

    protected override ExternalFileEntry CreateEntry(ulong id, uint blockSize, uint startBlock, uint languageId)
    {
        return new ExternalFileEntry
        {
            Id = id, BlockSize = blockSize, StartBlock = startBlock, LanguageId = languageId
        };
    }
}
