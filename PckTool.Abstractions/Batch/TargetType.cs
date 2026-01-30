namespace PckTool.Abstractions.Batch;

/// <summary>
///     Defines the type of target for a batch action.
/// </summary>
public enum TargetType
{
    /// <summary>
    ///     Target a WEM (audio) file.
    /// </summary>
    Wem,

    /// <summary>
    ///     Target a BNK (soundbank) file.
    /// </summary>
    Bnk
}
