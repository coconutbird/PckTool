namespace PckTool.Abstractions;

/// <summary>
///     Represents a Wwise soundbank (BNK file) containing audio hierarchy and media.
/// </summary>
/// <remarks>
///     Soundbanks are the core container for Wwise audio data. They contain:
///     <list type="bullet">
///         <item>
///             <description>Bank header (BKHD) with version and ID information</description>
///         </item>
///         <item>
///             <description>Hierarchy items (HIRC) defining audio objects and their relationships</description>
///         </item>
///         <item>
///             <description>Embedded media data (DATA) for sounds stored within the bank</description>
///         </item>
///         <item>
///             <description>Various metadata chunks (STID, STMG, ENVS, etc.)</description>
///         </item>
///     </list>
/// </remarks>
public interface ISoundBank
{
    /// <summary>
    ///     Gets the unique identifier for this soundbank.
    /// </summary>
    uint Id { get; }

    /// <summary>
    ///     Gets the Wwise version that created this soundbank.
    /// </summary>
    uint Version { get; }

    /// <summary>
    ///     Gets the language ID for localized soundbanks.
    /// </summary>
    uint LanguageId { get; }

    /// <summary>
    ///     Gets the number of embedded media entries in this soundbank.
    /// </summary>
    int MediaCount { get; }

    /// <summary>
    ///     Gets the number of HIRC (hierarchy) items in this soundbank.
    /// </summary>
    int HircItemCount { get; }

    /// <summary>
    ///     Determines whether this soundbank contains embedded media with the specified ID.
    /// </summary>
    /// <param name="sourceId">The source ID to check.</param>
    /// <returns>true if the media exists; otherwise, false.</returns>
    bool ContainsMedia(uint sourceId);

    /// <summary>
    ///     Gets embedded media by source ID.
    /// </summary>
    /// <param name="sourceId">The source ID of the media.</param>
    /// <returns>The media data if found; otherwise, null.</returns>
    byte[]? GetMedia(uint sourceId);

    /// <summary>
    ///     Replaces embedded media data and optionally updates HIRC size references.
    /// </summary>
    /// <param name="sourceId">The source ID of the media to replace.</param>
    /// <param name="data">The new media data.</param>
    /// <param name="updateHircSizes">Whether to update HIRC size references.</param>
    /// <returns>The number of HIRC references updated.</returns>
    int ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true);

    /// <summary>
    ///     Serializes this soundbank to a byte array.
    /// </summary>
    /// <returns>The serialized soundbank data.</returns>
    byte[] ToByteArray();
}
