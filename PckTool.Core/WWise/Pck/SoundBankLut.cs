using PckTool.Abstractions;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Lookup table for sound bank entries (uses 32-bit file IDs).
/// </summary>
public class SoundBankLut : FileLut<uint, SoundBankEntry>, ISoundBankCollection
{
    protected override int KeySize => 4;

    /// <inheritdoc />
    ISoundBankEntry? ISoundBankCollection.this[uint bankId] => this[bankId];

    /// <inheritdoc />
    IReadOnlyList<ISoundBankEntry> ISoundBankCollection.Entries => Entries.Cast<ISoundBankEntry>().ToList();

    /// <inheritdoc />
    IEnumerable<uint> ISoundBankCollection.BankIds => ById.Keys;

    /// <inheritdoc />
    bool ISoundBankCollection.TryGet(uint bankId, out ISoundBankEntry? entry)
    {
        var result = ById.TryGetValue(bankId, out var bankEntry);
        entry = bankEntry;

        return result;
    }

    /// <inheritdoc />
    IEnumerator<ISoundBankEntry> IEnumerable<ISoundBankEntry>.GetEnumerator()
    {
        foreach (var entry in Entries)
        {
            yield return entry;
        }
    }

    protected override uint ReadKey(BinaryReader reader)
    {
        return reader.ReadUInt32();
    }

    protected override void WriteKey(BinaryWriter writer, uint key)
    {
        writer.Write(key);
    }

    protected override SoundBankEntry CreateEntry(uint id, uint blockSize, uint startBlock, uint languageId)
    {
        return new SoundBankEntry { Id = id, BlockSize = blockSize, StartBlock = startBlock, LanguageId = languageId };
    }
}
