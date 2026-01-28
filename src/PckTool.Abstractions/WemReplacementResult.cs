namespace PckTool.Abstractions;

/// <summary>
/// Represents the result of a WEM file replacement operation.
/// </summary>
public sealed class WemReplacementResult
{
    /// <summary>
    /// Gets or sets the source ID of the replaced WEM.
    /// </summary>
    public uint SourceId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the WEM was replaced in streaming files.
    /// </summary>
    public bool ReplacedInStreaming { get; set; }

    /// <summary>
    /// Gets or sets the number of embedded soundbanks that were modified.
    /// </summary>
    public int EmbeddedBanksModified { get; set; }

    /// <summary>
    /// Gets or sets the total number of HIRC references that were updated.
    /// </summary>
    public int HircReferencesUpdated { get; set; }

    /// <summary>
    /// Gets a value indicating whether the WEM was found and replaced anywhere.
    /// </summary>
    public bool WasReplaced => ReplacedInStreaming || EmbeddedBanksModified > 0;

    /// <summary>
    /// Gets a summary of the replacement operation.
    /// </summary>
    public string Summary =>
        $"Source ID: 0x{SourceId:X8}, " +
        $"Streaming: {ReplacedInStreaming}, " +
        $"Banks Modified: {EmbeddedBanksModified}, " +
        $"HIRC Updated: {HircReferencesUpdated}";
}

