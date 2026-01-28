namespace PckTool.Abstractions;

/// <summary>
///     Represents a collection of external WEM files in a PCK package.
///     External files use 64-bit IDs and are stored directly in the PCK.
/// </summary>
public interface IExternalFileCollection : IEnumerable<IExternalFileEntry>
{
    /// <summary>
    ///     Gets the number of external files in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Gets all entries in insertion order (for LINQ operations).
    /// </summary>
    IReadOnlyList<IExternalFileEntry> Entries { get; }

    /// <summary>
    ///     Gets an external file entry by its ID.
    /// </summary>
    /// <param name="fileId">The 64-bit file ID.</param>
    /// <returns>The file entry if found; otherwise, null.</returns>
    IExternalFileEntry? this[ulong fileId] { get; }

    /// <summary>
    ///     Gets all file IDs in this collection.
    /// </summary>
    IEnumerable<ulong> FileIds { get; }

    /// <summary>
    ///     Determines whether an external file with the specified ID exists.
    /// </summary>
    /// <param name="fileId">The file ID to check.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
    bool Contains(ulong fileId);

    /// <summary>
    ///     Tries to get an external file entry.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <param name="entry">The entry if found.</param>
    /// <returns>true if found; otherwise, false.</returns>
    bool TryGet(ulong fileId, out IExternalFileEntry? entry);
}
