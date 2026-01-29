namespace PckTool.Abstractions;

/// <summary>
///     Represents a streaming WEM file entry in a PCK package.
/// </summary>
public interface IStreamingFileEntry
{
    /// <summary>
    ///     Gets the file ID (same as SourceId, for compatibility).
    /// </summary>
    uint Id { get; }

    /// <summary>
    ///     Gets the Wwise source ID for this WEM file.
    /// </summary>
    uint SourceId { get; }

    /// <summary>
    ///     Gets the size of the WEM data in bytes.
    /// </summary>
    uint Size { get; }

    /// <summary>
    ///     Gets the language ID for localized files (0 for SFX).
    /// </summary>
    uint LanguageId { get; }

    /// <summary>
    ///     Gets the WEM file data.
    /// </summary>
    /// <returns>The raw WEM data.</returns>
    byte[] GetData();

    /// <summary>
    ///     Replaces the WEM data with new content.
    /// </summary>
    /// <param name="data">The new WEM data.</param>
    void ReplaceWith(byte[] data);
}
