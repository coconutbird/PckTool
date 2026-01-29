namespace PckTool.Abstractions;

/// <summary>
///     Represents a collection of soundbank entries in a PCK package.
/// </summary>
public interface ISoundBankCollection : IEnumerable<ISoundBankEntry>
{
    /// <summary>
    ///     Gets the number of soundbanks in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Gets all entries in insertion order (for LINQ operations).
    /// </summary>
    IReadOnlyList<ISoundBankEntry> Entries { get; }

    /// <summary>
    ///     Gets a soundbank entry by its ID.
    /// </summary>
    /// <param name="bankId">The soundbank ID.</param>
    /// <returns>The soundbank entry if found; otherwise, null.</returns>
    ISoundBankEntry? this[uint bankId] { get; }

    /// <summary>
    ///     Gets all soundbank IDs in this collection.
    /// </summary>
    IEnumerable<uint> BankIds { get; }

    /// <summary>
    ///     Determines whether a soundbank with the specified ID exists.
    /// </summary>
    /// <param name="bankId">The bank ID to check.</param>
    /// <returns>true if the soundbank exists; otherwise, false.</returns>
    bool Contains(uint bankId);

    /// <summary>
    ///     Tries to get a soundbank entry.
    /// </summary>
    /// <param name="bankId">The bank ID.</param>
    /// <param name="entry">The entry if found.</param>
    /// <returns>true if found; otherwise, false.</returns>
    bool TryGet(uint bankId, out ISoundBankEntry? entry);
}
