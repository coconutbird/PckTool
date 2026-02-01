namespace PckTool.Abstractions;

/// <summary>
///     Represents a unified audio file that can be either a PCK package or a standalone BNK soundbank.
/// </summary>
/// <remarks>
///     This interface provides a common abstraction for working with both:
///     <list type="bullet">
///         <item>
///             <description>PCK files - containers with multiple soundbanks and streaming files</description>
///         </item>
///         <item>
///             <description>BNK files - standalone soundbanks with embedded media</description>
///         </item>
///     </list>
/// </remarks>
public interface IAudioFile : IDisposable
{
    /// <summary>
    ///     Gets the source file path this audio file was loaded from, if applicable.
    /// </summary>
    string? SourcePath { get; }

    /// <summary>
    ///     Gets whether any entries have been modified since loading.
    /// </summary>
    bool HasModifications { get; }

    /// <summary>
    ///     Gets the type of audio file (PCK or BNK).
    /// </summary>
    AudioFileType FileType { get; }

    /// <summary>
    ///     Gets the soundbank entries in this audio file.
    ///     For BNK files, this returns a single-entry collection.
    /// </summary>
    ISoundBankCollection SoundBanks { get; }

    /// <summary>
    ///     Gets the streaming file entries in this audio file.
    ///     For BNK files, this returns an empty collection.
    /// </summary>
    IStreamingFileCollection StreamingFiles { get; }

    /// <summary>
    ///     Gets the external file entries in this audio file.
    ///     For BNK files, this returns an empty collection.
    /// </summary>
    IExternalFileCollection ExternalFiles { get; }

    /// <summary>
    ///     Gets the language ID to name mapping.
    ///     For BNK files, this returns a single-entry mapping.
    /// </summary>
    IReadOnlyDictionary<uint, string> Languages { get; }

    /// <summary>
    ///     Gets the language name for a language ID.
    /// </summary>
    /// <param name="languageId">The language ID.</param>
    /// <returns>The language name, or null if not found.</returns>
    string? GetLanguageName(uint languageId);

    /// <summary>
    ///     Gets the number of soundbanks contained in this audio file.
    /// </summary>
    int SoundBankCount { get; }

    /// <summary>
    ///     Gets the number of streaming WEM files in this audio file.
    /// </summary>
    int StreamingFileCount { get; }

    /// <summary>
    ///     Gets the number of external WEM files in this audio file.
    /// </summary>
    int ExternalFileCount { get; }

    /// <summary>
    ///     Finds a WEM file by its source ID across all storage locations.
    /// </summary>
    /// <param name="sourceId">The Wwise source ID of the WEM file.</param>
    /// <returns>The WEM data if found; otherwise, null.</returns>
    byte[]? FindWem(uint sourceId);

    /// <summary>
    ///     Determines whether a WEM with the specified source ID exists.
    /// </summary>
    /// <param name="sourceId">The Wwise source ID to search for.</param>
    /// <returns>true if the WEM exists; otherwise, false.</returns>
    bool ContainsWem(uint sourceId);

    /// <summary>
    ///     Replaces a WEM file's data across all locations where it exists.
    /// </summary>
    /// <param name="sourceId">The source ID of the WEM to replace.</param>
    /// <param name="data">The new WEM data.</param>
    /// <param name="updateHircSizes">Whether to update HIRC size references.</param>
    /// <returns>A result describing what was replaced.</returns>
    WemReplacementResult ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true);

    /// <summary>
    ///     Saves the audio file to the specified path.
    /// </summary>
    /// <param name="path">The file path to save to.</param>
    void Save(string path);

    /// <summary>
    ///     Saves the audio file to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    void Save(Stream stream);
}

/// <summary>
///     Specifies the type of audio file.
/// </summary>
public enum AudioFileType
{
    /// <summary>
    ///     A PCK package file containing multiple soundbanks and streaming files.
    /// </summary>
    Pck,

    /// <summary>
    ///     A standalone BNK soundbank file.
    /// </summary>
    Bnk
}
