using System.Collections;

using PckTool.Abstractions;

namespace PckTool.Core.WWise.Bnk.Collections;

/// <summary>
///     A collection containing a single soundbank entry, used for standalone BNK files.
/// </summary>
internal class BnkFileSoundBankCollection : ISoundBankCollection
{
    private readonly ISoundBankEntry _entry;

    public BnkFileSoundBankCollection(ISoundBankEntry entry)
    {
        _entry = entry;
        Entries = new List<ISoundBankEntry> { entry };
    }

    /// <inheritdoc />
    public int Count => 1;

    /// <inheritdoc />
    public IReadOnlyList<ISoundBankEntry> Entries { get; }

    /// <inheritdoc />
    public ISoundBankEntry? this[uint bankId] => bankId == _entry.Id ? _entry : null;

    /// <inheritdoc />
    public IEnumerable<uint> BankIds => new[] { _entry.Id };

    /// <inheritdoc />
    public bool Contains(uint bankId)
    {
        return bankId == _entry.Id;
    }

    /// <inheritdoc />
    public bool TryGet(uint bankId, out ISoundBankEntry? entry)
    {
        if (bankId == _entry.Id)
        {
            entry = _entry;

            return true;
        }

        entry = null;

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<ISoundBankEntry> GetEnumerator()
    {
        yield return _entry;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
