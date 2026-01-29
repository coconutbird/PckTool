using PckTool.Abstractions;

namespace PckTool.Core.WWise.Pck.Entries;

/// <summary>
///     Represents an external file entry in a package file.
///     Uses 64-bit file IDs.
/// </summary>
public class ExternalFileEntry : FileEntry<ulong>, IExternalFileEntry
{
    /// <summary>
    ///     Human-readable name (if known).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The language name (resolved from LanguageMap).
    /// </summary>
    public string? Language { get; set; }

    /// <inheritdoc />
    ulong IExternalFileEntry.Id => Id;

    /// <inheritdoc />
    uint IExternalFileEntry.Size => (uint) base.Size;

    public override string ToString()
    {
        var name = Name ?? $"0x{Id:X16}";
        var lang = Language ?? $"Lang:{LanguageId}";

        return $"{name} ({lang})";
    }
}
