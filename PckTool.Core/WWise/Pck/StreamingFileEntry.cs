using PckTool.Abstractions;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Represents a streaming audio file (.wem) entry in a package file.
/// </summary>
public class StreamingFileEntry : FileEntry<uint>, IStreamingFileEntry
{
    /// <summary>
    ///     Human-readable name (if resolved from cue table).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The language name (resolved from LanguageMap).
    /// </summary>
    public string? Language { get; set; }

    /// <inheritdoc />
    uint IStreamingFileEntry.Id => Id;

    /// <inheritdoc />
    uint IStreamingFileEntry.SourceId => Id;

    /// <inheritdoc />
    uint IStreamingFileEntry.Size => (uint) base.Size;

    public override string ToString()
    {
        var name = Name ?? $"0x{Id:X8}";
        var lang = Language ?? $"Lang:{LanguageId}";

        return $"{name} ({lang})";
    }
}
