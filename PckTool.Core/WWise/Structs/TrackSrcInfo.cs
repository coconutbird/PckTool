namespace PckTool.Core.WWise.Structs;

/// <summary>
///     Track source info (AkTrackSrcInfo) for MusicTrack playlist.
///     For v27+ (v113 falls in v27-132 range).
/// </summary>
public class TrackSrcInfo
{
    /// <summary>
    ///     Track ID (0..N).
    /// </summary>
    public uint TrackId { get; set; }

    /// <summary>
    ///     Source ID (tid).
    /// </summary>
    public uint SourceId { get; set; }

    /// <summary>
    ///     Play position (double).
    /// </summary>
    public double PlayAt { get; set; }

    /// <summary>
    ///     Begin trim offset (double).
    /// </summary>
    public double BeginTrimOffset { get; set; }

    /// <summary>
    ///     End trim offset (double).
    /// </summary>
    public double EndTrimOffset { get; set; }

    /// <summary>
    ///     Source duration (double).
    /// </summary>
    public double SrcDuration { get; set; }

    public bool Read(BinaryReader reader)
    {
        // For v27-132 (v113 is in this range):
        // trackID (u32), sourceID (tid), fPlayAt (d64), fBeginTrimOffset (d64), 
        // fEndTrimOffset (d64), fSrcDuration (d64)
        TrackId = reader.ReadUInt32();
        SourceId = reader.ReadUInt32();
        PlayAt = reader.ReadDouble();
        BeginTrimOffset = reader.ReadDouble();
        EndTrimOffset = reader.ReadDouble();
        SrcDuration = reader.ReadDouble();

        return true;
    }
}
