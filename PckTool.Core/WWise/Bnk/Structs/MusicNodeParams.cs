namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     Music node params (CAkMusicNode::SetMusicNodeParams).
///     Base params shared by MusicSegment, MusicSwitch, and MusicRanSeq.
/// </summary>
public class MusicNodeParams
{
    /// <summary>
    ///     Flags for v90+ (MIDI override settings).
    /// </summary>
    public byte Flags { get; set; }

    /// <summary>
    ///     Node base params (inherited from CAkParameterNodeBase).
    /// </summary>
    public NodeBaseParams NodeBaseParams { get; set; } = null!;

    /// <summary>
    ///     Child node IDs.
    /// </summary>
    public Children Children { get; set; } = null!;

    /// <summary>
    ///     Meter info (AkMeterInfo).
    /// </summary>
    public MeterInfo MeterInfo { get; set; } = null!;

    /// <summary>
    ///     Override meter flag (0=off, !0=on).
    /// </summary>
    public byte MeterInfoFlag { get; set; }

    /// <summary>
    ///     Stingers list (CAkStinger).
    /// </summary>
    public List<Stinger> Stingers { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // For v90+: uFlags (u8)
        Flags = reader.ReadByte();

        // NodeBaseParams
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;

        // Children
        var children = new Children();

        if (!children.Read(reader))
        {
            return false;
        }

        Children = children;

        // AkMeterInfo
        var meterInfo = new MeterInfo
        {
            GridPeriod = reader.ReadDouble(),
            GridOffset = reader.ReadDouble(),
            Tempo = reader.ReadSingle(),
            TimeSigNumBeatsBar = reader.ReadByte(),
            TimeSigBeatValue = reader.ReadByte()
        };

        MeterInfo = meterInfo;

        // bMeterInfoFlag (u8)
        MeterInfoFlag = reader.ReadByte();

        // NumStingers + stinger list
        var numStingers = reader.ReadUInt32();

        for (var i = 0; i < numStingers; i++)
        {
            var stinger = new Stinger();

            if (!stinger.Read(reader))
            {
                return false;
            }

            Stingers.Add(stinger);
        }

        return true;
    }
}

/// <summary>
///     Meter info (AkMeterInfo).
/// </summary>
public class MeterInfo
{
    public double GridPeriod { get; set; }
    public double GridOffset { get; set; }
    public float Tempo { get; set; }
    public byte TimeSigNumBeatsBar { get; set; }
    public byte TimeSigBeatValue { get; set; }
}

/// <summary>
///     Stinger (CAkStinger).
/// </summary>
public class Stinger
{
    public uint TriggerId { get; set; }
    public uint SegmentId { get; set; }
    public uint SyncPlayAt { get; set; }
    public uint CueFilterHash { get; set; }
    public int DontRepeatTime { get; set; }
    public uint NumSegmentLookAhead { get; set; }

    public bool Read(BinaryReader reader)
    {
        // For v65+ (v113 is in range):
        TriggerId = reader.ReadUInt32();
        SegmentId = reader.ReadUInt32();
        SyncPlayAt = reader.ReadUInt32();
        CueFilterHash = reader.ReadUInt32();
        DontRepeatTime = reader.ReadInt32();
        NumSegmentLookAhead = reader.ReadUInt32();

        return true;
    }
}
