namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Music transition node params (CAkMusicTransAware::SetMusicTransNodeParams).
///     Used by MusicSwitch and MusicRanSeq.
/// </summary>
public class MusicTransNodeParams
{
    /// <summary>
    ///     Music node params.
    /// </summary>
    public MusicNodeParams MusicNodeParams { get; set; } = null!;

    /// <summary>
    ///     Transition rules (AkMusicTransitionRule).
    /// </summary>
    public List<MusicTransitionRule> Rules { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // MusicNodeParams
        var musicNodeParams = new MusicNodeParams();

        if (!musicNodeParams.Read(reader))
        {
            return false;
        }

        MusicNodeParams = musicNodeParams;

        // numRules + pRules
        var numRules = reader.ReadUInt32();

        for (var i = 0; i < numRules; i++)
        {
            var rule = new MusicTransitionRule();

            if (!rule.Read(reader))
            {
                return false;
            }

            Rules.Add(rule);
        }

        return true;
    }
}

/// <summary>
///     Music transition rule (AkMusicTransitionRule).
///     For v73+ (v113 is in range).
/// </summary>
public class MusicTransitionRule
{
    public List<uint> SrcIds { get; set; } = [];
    public List<uint> DstIds { get; set; } = [];
    public MusicTransSrcRule SrcRule { get; set; } = null!;
    public MusicTransDstRule DstRule { get; set; } = null!;
    public byte AllocTransObjectFlag { get; set; }
    public MusicTransitionObject? TransObject { get; set; }

    public bool Read(BinaryReader reader)
    {
        // For v73+: uNumSrc + srcIDs
        var numSrc = reader.ReadUInt32();

        for (var i = 0; i < numSrc; i++)
        {
            SrcIds.Add(reader.ReadUInt32());
        }

        // uNumDst + dstIDs
        var numDst = reader.ReadUInt32();

        for (var i = 0; i < numDst; i++)
        {
            DstIds.Add(reader.ReadUInt32());
        }

        // AkMusicTransSrcRule
        SrcRule = new MusicTransSrcRule
        {
            TransitionTime = reader.ReadInt32(),
            FadeCurve = reader.ReadUInt32(),
            FadeOffset = reader.ReadInt32(),
            SyncType = reader.ReadUInt32(),
            CueFilterHash = reader.ReadUInt32(),
            PlayPostExit = reader.ReadByte()
        };

        // AkMusicTransDstRule
        DstRule = new MusicTransDstRule
        {
            TransitionTime = reader.ReadInt32(),
            FadeCurve = reader.ReadUInt32(),
            FadeOffset = reader.ReadInt32(),
            CueFilterHash = reader.ReadUInt32(),
            JumpToId = reader.ReadUInt32(),
            EntryType = reader.ReadUInt16(),
            PlayPreEntry = reader.ReadByte(),
            DestMatchSourceCueName = reader.ReadByte()
        };

        // AllocTransObjectFlag (u8)
        AllocTransObjectFlag = reader.ReadByte();

        if (AllocTransObjectFlag != 0)
        {
            var transObj = new MusicTransitionObject();

            if (!transObj.Read(reader))
            {
                return false;
            }

            TransObject = transObj;
        }

        return true;
    }
}
