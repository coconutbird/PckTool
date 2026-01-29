namespace PckTool.Abstractions;

/// <summary>
///     Represents a collection of streaming WEM files in a PCK package.
/// </summary>
public interface IStreamingFileCollection : IEnumerable<IStreamingFileEntry>
{
    /// <summary>
    ///     Gets the number of streaming files in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    ///     Gets all entries in insertion order (for LINQ operations).
    /// </summary>
    IReadOnlyList<IStreamingFileEntry> Entries { get; }

    /// <summary>
    ///     Gets a streaming file entry by its source ID.
    /// </summary>
    /// <param name="sourceId">The Wwise source ID.</param>
    /// <returns>The file entry if found; otherwise, null.</returns>
    IStreamingFileEntry? this[uint sourceId] { get; }

    /// <summary>
    ///     Gets all source IDs in this collection.
    /// </summary>
    IEnumerable<uint> SourceIds { get; }

    /// <summary>
    ///     Determines whether a streaming file with the specified ID exists.
    /// </summary>
    /// <param name="sourceId">The source ID to check.</param>
    /// <returns>true if the file exists; otherwise, false.</returns>
    bool Contains(uint sourceId);

    /// <summary>
    ///     Tries to get a streaming file entry.
    /// </summary>
    /// <param name="sourceId">The source ID.</param>
    /// <param name="entry">The entry if found.</param>
    /// <returns>true if found; otherwise, false.</returns>
    bool TryGet(uint sourceId, out IStreamingFileEntry? entry);
}
