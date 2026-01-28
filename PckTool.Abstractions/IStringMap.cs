namespace PckTool.Abstractions;

/// <summary>
///     Represents a string map containing folder names and other metadata in a PCK file.
/// </summary>
public interface IStringMap : IEnumerable<KeyValuePair<uint, string>>
{
    /// <summary>
    ///     Gets the number of entries in the string map.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Gets a string by its hash.
    /// </summary>
    /// <param name="hash">The FNV hash of the string.</param>
    /// <returns>The string if found; otherwise, null.</returns>
    string? this[uint hash] { get; }

    /// <summary>
    ///     Determines whether the map contains an entry with the specified hash.
    /// </summary>
    /// <param name="hash">The hash to check.</param>
    /// <returns>true if the entry exists; otherwise, false.</returns>
    bool Contains(uint hash);

    /// <summary>
    ///     Tries to get a string by its hash.
    /// </summary>
    /// <param name="hash">The hash.</param>
    /// <param name="value">The string if found.</param>
    /// <returns>true if found; otherwise, false.</returns>
    bool TryGet(uint hash, out string? value);

    /// <summary>
    ///     Adds an entry to the string map.
    /// </summary>
    /// <param name="hash">The FNV hash of the string.</param>
    /// <param name="value">The string value.</param>
    void Add(uint hash, string value);

    /// <summary>
    ///     Removes an entry from the string map.
    /// </summary>
    /// <param name="hash">The hash of the entry to remove.</param>
    /// <returns>true if removed; otherwise, false.</returns>
    bool Remove(uint hash);
}
