namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Track transition params (TrackSwitchInfo::SetTransParams) for MusicTrack.
///     Only present when TrackType == 0x3 (Switch).
/// </summary>
public class TrackTransParams
{
    /// <summary>
    ///     Source fade params (AkMusicFade).
    /// </summary>
    public MusicFade SrcFadeParams { get; set; } = null!;

    /// <summary>
    ///     Sync type (AkSyncType).
    /// </summary>
    public uint SyncType { get; set; }

    /// <summary>
    ///     Cue filter hash.
    /// </summary>
    public uint CueFilterHash { get; set; }

    /// <summary>
    ///     Destination fade params (AkMusicFade).
    /// </summary>
    public MusicFade DestFadeParams { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // srcFadeParams (AkMusicFade)
        SrcFadeParams = new MusicFade
        {
            TransitionTime = reader.ReadInt32(), FadeCurve = reader.ReadUInt32(), FadeOffset = reader.ReadInt32()
        };

        // eSyncType (u32)
        SyncType = reader.ReadUInt32();

        // uCueFilterHash (u32)
        CueFilterHash = reader.ReadUInt32();

        // destFadeParams (AkMusicFade)
        DestFadeParams = new MusicFade
        {
            TransitionTime = reader.ReadInt32(), FadeCurve = reader.ReadUInt32(), FadeOffset = reader.ReadInt32()
        };

        return true;
    }
}

/// <summary>
///     Music fade params (AkMusicFade).
/// </summary>
public class MusicFade
{
    /// <summary>
    ///     Transition time in milliseconds.
    /// </summary>
    public int TransitionTime { get; set; }

    /// <summary>
    ///     Fade curve type (AkCurveInterpolation).
    /// </summary>
    public uint FadeCurve { get; set; }

    /// <summary>
    ///     Fade offset in milliseconds.
    /// </summary>
    public int FadeOffset { get; set; }
}
