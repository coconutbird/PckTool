using System.Text.Json;
using System.Text.Json.Serialization;

namespace PckTool.Core.HaloWars;

/// <summary>
///     Metadata for WEM files extracted from a soundbank.
///     Contains the mapping between WEM file IDs and their associated cue names.
/// </summary>
public class WemMetadata
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    ///     The soundbank ID this metadata belongs to.
    /// </summary>
    public uint SoundbankId { get; set; }

    /// <summary>
    ///     The soundbank ID in hexadecimal format.
    /// </summary>
    public string SoundbankIdHex => $"{SoundbankId:X8}";

    /// <summary>
    ///     The language of the soundbank.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    ///     The language ID of the soundbank.
    /// </summary>
    public uint LanguageId { get; set; }

    /// <summary>
    ///     The list of WEM files with their metadata.
    /// </summary>
    public List<WemFileEntry> Files { get; set; } = [];

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
    public static WemMetadata? Load(string path)
    {
        if (!File.Exists(path)) return null;

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<WemMetadata>(stream, JsonOptions);
    }
}

/// <summary>
///     Represents a single WEM file entry in the metadata.
/// </summary>
public class WemFileEntry
{
    /// <summary>
    ///     The WEM file ID (source ID).
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
///     Represents a cue (event) reference in the WEM metadata.
///     Captures cross-bank relationships by tracking where the event is defined.
/// </summary>
public class CueMetadata
{
    /// <summary>
    ///     The human-readable cue name (event name).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The cue index (FNV1A-32 hash of the cue name / event ID).
    /// </summary>
    public uint EventId { get; set; }

    /// <summary>
    ///     The cue index in hexadecimal format.
    /// </summary>
    public string EventIdHex => $"{EventId:X8}";

    /// <summary>
    ///     The ID of the soundbank where this event is defined.
    /// </summary>
    public uint SourceBankId { get; set; }

    /// <summary>
    ///     The source bank ID in hexadecimal format.
    /// </summary>
    public string SourceBankIdHex => $"{SourceBankId:X8}";
}
