namespace PckTool.Core.WWise.Structs;

/// <summary>
///     Music transition source rule (AkMusicTransSrcRule).
///     For v73+ (v113 is in range).
/// </summary>
public class MusicTransSrcRule
{
    public int TransitionTime { get; set; }
    public uint FadeCurve { get; set; }
    public int FadeOffset { get; set; }
    public uint SyncType { get; set; }
    public uint CueFilterHash { get; set; }
    public byte PlayPostExit { get; set; }
}

/// <summary>
///     Music transition destination rule (AkMusicTransDstRule).
///     For v73-132 (v113 is in range).
/// </summary>
public class MusicTransDstRule
{
    public int TransitionTime { get; set; }
    public uint FadeCurve { get; set; }
    public int FadeOffset { get; set; }
    public uint CueFilterHash { get; set; }
    public uint JumpToId { get; set; }
    public ushort EntryType { get; set; }
    public byte PlayPreEntry { get; set; }
    public byte DestMatchSourceCueName { get; set; }
}

/// <summary>
///     Music transition object (AkMusicTransitionObject).
///     For v35+ (v113 is in range).
/// </summary>
public class MusicTransitionObject
{
    public uint SegmentId { get; set; }
    public MusicFade FadeInParams { get; set; } = null!;
    public MusicFade FadeOutParams { get; set; } = null!;
    public byte PlayPreEntry { get; set; }
    public byte PlayPostExit { get; set; }

    public bool Read(BinaryReader reader)
    {
        // segmentID (tid)
        SegmentId = reader.ReadUInt32();

        // fadeInParams (AkMusicFade)
        FadeInParams = new MusicFade
        {
            TransitionTime = reader.ReadInt32(), FadeCurve = reader.ReadUInt32(), FadeOffset = reader.ReadInt32()
        };

        // fadeOutParams (AkMusicFade)
        FadeOutParams = new MusicFade
        {
            TransitionTime = reader.ReadInt32(), FadeCurve = reader.ReadUInt32(), FadeOffset = reader.ReadInt32()
        };

        // bPlayPreEntry, bPlayPostExit
        PlayPreEntry = reader.ReadByte();
        PlayPostExit = reader.ReadByte();

        return true;
    }
}
