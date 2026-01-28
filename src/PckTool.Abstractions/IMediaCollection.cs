namespace PckTool.Abstractions;

/// <summary>
/// Represents a collection of embedded media within a soundbank.
/// </summary>
public interface IMediaCollection : IEnumerable<KeyValuePair<uint, byte[]>>
{
    /// <summary>
    /// Gets the number of media entries in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets or sets media data by source ID.
    /// </summary>
    /// <param name="sourceId">The Wwise source ID.</param>
    /// <returns>The media data.</returns>
    /// <exception cref="KeyNotFoundException">The source ID was not found.</exception>
    byte[] this[uint sourceId] { get; set; }

    /// <summary>
    /// Determines whether the collection contains media with the specified ID.
    /// </summary>
    /// <param name="sourceId">The source ID to check.</param>
    /// <returns>true if the media exists; otherwise, false.</returns>
    bool Contains(uint sourceId);

    /// <summary>
    /// Tries to get media data by source ID.
    /// </summary>
    /// <param name="sourceId">The source ID.</param>
    /// <param name="data">The media data if found.</param>
    /// <returns>true if found; otherwise, false.</returns>
    bool TryGet(uint sourceId, out byte[]? data);

    /// <summary>
    /// Adds media to the collection.
    /// </summary>
    /// <param name="sourceId">The source ID.</param>
    /// <param name="data">The media data.</param>
    void Add(uint sourceId, byte[] data);

    /// <summary>
    /// Removes media from the collection.
    /// </summary>
    /// <param name="sourceId">The source ID to remove.</param>
    /// <returns>true if removed; otherwise, false.</returns>
    bool Remove(uint sourceId);

    /// <summary>
    /// Gets all source IDs in this collection.
    /// </summary>
    IEnumerable<uint> SourceIds { get; }
}

