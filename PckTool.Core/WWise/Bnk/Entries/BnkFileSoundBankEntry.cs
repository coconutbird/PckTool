using PckTool.Abstractions;

namespace PckTool.Core.WWise.Bnk.Entries;

/// <summary>
///     Wraps a SoundBank to implement ISoundBankEntry for standalone BNK files.
/// </summary>
internal class BnkFileSoundBankEntry : ISoundBankEntry
{
    private readonly BnkFile _parent;
    private readonly SoundBank _soundBank;

    public BnkFileSoundBankEntry(SoundBank soundBank, BnkFile parent)
    {
        _soundBank = soundBank;
        _parent = parent;
    }

    /// <inheritdoc />
    public uint Id => _soundBank.Id;

    /// <inheritdoc />
    public uint LanguageId => _soundBank.LanguageId;

    /// <inheritdoc />
    public uint Size => (uint) _soundBank.ToByteArray().Length;

    /// <inheritdoc />
    public byte[] GetData()
    {
        return _soundBank.ToByteArray();
    }

    /// <inheritdoc />
    public ISoundBank? Parse()
    {
        return _soundBank;
    }

    /// <inheritdoc />
    public void ReplaceWith(byte[] data)
    {
        // For standalone BNK, we need to re-parse the data into the soundbank
        // This is a bit unusual but maintains consistency with the interface
        var newBank = SoundBank.Parse(data);

        if (newBank is null)
        {
            throw new InvalidDataException("Failed to parse replacement BNK data.");
        }

        // Copy properties from new bank to existing bank
        // Note: This is a simplified approach - in practice you might want to
        // replace the entire soundbank reference
        _soundBank.Media.Clear();

        foreach (var kvp in newBank.Media)
        {
            _soundBank.Media.Add(kvp.Key, kvp.Value);
        }

        _soundBank.Items.Clear();
        _soundBank.Items.AddRange(newBank.Items);

        _parent.MarkModified();
    }

    /// <inheritdoc />
    public void ReplaceWith(ISoundBank soundBank)
    {
        ReplaceWith(soundBank.ToByteArray());
    }
}
