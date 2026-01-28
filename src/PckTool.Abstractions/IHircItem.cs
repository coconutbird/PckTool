namespace PckTool.Abstractions;

/// <summary>
/// Represents a hierarchy (HIRC) item in a Wwise soundbank.
/// </summary>
/// <remarks>
/// HIRC items define the audio object hierarchy and include sounds, events,
/// actions, containers, and other audio constructs. This interface provides
/// read-only access to basic HIRC item properties.
/// </remarks>
public interface IHircItem
{
    /// <summary>
    /// Gets the type of this HIRC item as a byte value.
    /// </summary>
    /// <remarks>
    /// Common types include: Sound (2), Action (3), Event (4), etc.
    /// See the Wwise documentation for a complete list.
    /// </remarks>
    byte Type { get; }

    /// <summary>
    /// Gets the unique identifier for this HIRC item within the soundbank.
    /// </summary>
    uint Id { get; }
}

