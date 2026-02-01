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
        ParentFile = parent;
    }

    /// <inheritdoc />
    public uint Id => _soundBank.Id;

    /// <inheritdoc />
    public uint LanguageId => _soundBank.LanguageId;

    /// <inheritdoc />
    public uint Size => (uint) _soundBank.ToByteArray().Length;

    /// <inheritdoc />
    public IAudioFile? ParentFile { get; set; }

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

        // Only copy content (Media and Items), not identity metadata (Id, LanguageId, etc.)
        // This is intentional - we're replacing the soundbank's content while preserving
        // its identity so it remains in the same slot within the parent file.
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
