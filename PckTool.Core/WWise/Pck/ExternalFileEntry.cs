namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Represents an external file entry in a package file.
///     Uses 64-bit file IDs.
/// </summary>
public class ExternalFileEntry : FileEntry<ulong>
{
    /// <summary>
    ///     Human-readable name (if known).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The language name (resolved from LanguageMap).
    /// </summary>
    public string? Language { get; set; }

    public override string ToString()
    {
        var name = Name ?? $"0x{Id:X16}";
        var lang = Language ?? $"Lang:{LanguageId}";

        return $"{name} ({lang})";
    }
}
