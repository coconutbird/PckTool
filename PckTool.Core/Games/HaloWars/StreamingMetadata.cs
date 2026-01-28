using System.Text.Json;
using System.Text.Json.Serialization;

namespace PckTool.Core.Games.HaloWars;

/// <summary>
///     Metadata for streaming WEM files extracted from a PCK package.
///     These are WEM files stored directly in the PCK, not embedded in BNK files.
/// </summary>
public class StreamingMetadata
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    ///     The language of the streaming files.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    ///     The language ID.
    /// </summary>
    public uint LanguageId { get; set; }

    /// <summary>
    ///     The list of streaming WEM files with their metadata.
    /// </summary>
    public List<StreamingFileMetadataEntry> Files { get; set; } = [];

    /// <summary>
    ///     Saves the metadata to a JSON file.
    /// </summary>
    public void Save(string path)
    {
        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, this, JsonOptions);
    }

    /// <summary>
    ///     Serializes the metadata to a JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    /// <summary>
    ///     Loads metadata from a JSON file.
    /// </summary>
    public static StreamingMetadata? Load(string path)
    {
        if (!File.Exists(path)) return null;

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<StreamingMetadata>(stream, JsonOptions);
    }
}

/// <summary>
///     Represents a single streaming WEM file entry in the metadata.
/// </summary>
public class StreamingFileMetadataEntry
{
    /// <summary>
    ///     The WEM file ID (32-bit).
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    ///     The WEM file ID in hexadecimal format.
    /// </summary>
    public string IdHex => $"{Id:X8}";

    /// <summary>
    ///     The filename of the extracted WEM file.
    /// </summary>
    public string FileName => $"{Id}.wem";

    /// <summary>
    ///     The file size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///     The cue references (events) that use this WEM file.
    ///     Contains full information about each cue including which bank it's defined in.
    /// </summary>
    public List<CueMetadata> Cues { get; set; } = [];

    /// <summary>
    ///     Whether this WEM file has any associated cues.
    /// </summary>
    [JsonIgnore] public bool HasCues => Cues.Count > 0;
}

/// <summary>
///     Metadata for external files extracted from a PCK package.
///     These use 64-bit file IDs.
/// </summary>
public class ExternalMetadata
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    ///     The language of the external files.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    ///     The language ID.
    /// </summary>
    public uint LanguageId { get; set; }

    /// <summary>
    ///     The list of external files with their metadata.
    /// </summary>
    public List<ExternalFileMetadataEntry> Files { get; set; } = [];

    /// <summary>
    ///     Saves the metadata to a JSON file.
    /// </summary>
    public void Save(string path)
    {
        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, this, JsonOptions);
    }

    /// <summary>
    ///     Serializes the metadata to a JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    /// <summary>
    ///     Loads metadata from a JSON file.
    /// </summary>
    public static ExternalMetadata? Load(string path)
    {
        if (!File.Exists(path)) return null;

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<ExternalMetadata>(stream, JsonOptions);
    }
}

/// <summary>
///     Represents a single external file entry in the metadata.
/// </summary>
public class ExternalFileMetadataEntry
{
    /// <summary>
    ///     The file ID (64-bit).
    /// </summary>
    public ulong Id { get; set; }

    /// <summary>
    ///     The file ID in hexadecimal format.
    /// </summary>
    public string IdHex => $"{Id:X16}";

    /// <summary>
    ///     The filename of the extracted file.
    /// </summary>
    public string FileName => $"{Id}.wem";

    /// <summary>
    ///     The file size in bytes.
    /// </summary>
    public long Size { get; set; }
}
