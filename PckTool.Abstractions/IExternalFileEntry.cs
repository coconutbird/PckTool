namespace PckTool.Abstractions;

/// <summary>
///     Represents an external WEM file entry in a PCK package.
///     External files use 64-bit IDs (unlike streaming files which use 32-bit).
/// </summary>
public interface IExternalFileEntry
{
    /// <summary>
    ///     Gets the file ID (64-bit).
    /// </summary>
    ulong Id { get; }

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
