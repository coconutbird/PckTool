namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Result of a WEM replacement operation.
/// </summary>
public class WemReplacementResult
{
    /// <summary>
    ///     The WEM source ID that was replaced.
    /// </summary>
    public uint SourceId { get; set; }

    /// <summary>
    ///     True if the WEM was found and replaced in streaming files.
    /// </summary>
    public bool ReplacedInStreaming { get; set; }

    /// <summary>
    ///     Number of soundbanks where embedded media was replaced.
    /// </summary>
    public int EmbeddedBanksModified { get; set; }

    /// <summary>
    ///     Total number of HIRC references updated across all locations.
    /// </summary>
    public int HircReferencesUpdated { get; set; }

    /// <summary>
    ///     True if any replacement was made (either streaming or embedded).
    /// </summary>
    public bool WasReplaced => ReplacedInStreaming || EmbeddedBanksModified > 0;
}
