namespace SoundsUnpack.WWise.Structs;

public class RanSeqCntrInitialValues
{
    public NodeBaseParams NodeBaseParams { get; set; }
    public ushort LoopCount { get; set; }
    public ushort LoopModMin { get; set; }
    public ushort LoopModMax { get; set; }
    public float TransitionTime { get; set; }
    public float TransitionTimeModMin { get; set; }
    public float TransitionTimeModMax { get; set; }
    public ushort AvoidRepeatCount { get; set; }
    public byte TransitionMode { get; set; }
    public byte RandomMode { get; set; }
    public byte Mode { get; set; }
    public byte BitVector { get; set; }

    public bool IsUsingWeight
    {
        get => (BitVector & 0x01) != 0;
        set
        {
            if (value)
            {
                BitVector |= 0x01;
            }
            else
            {
                BitVector &= 0xFE;
            }
        }
    }

    public bool ResetPlayListAtEachPlay
    {
        get => (BitVector & 0x02) != 0;
        set
        {
            if (value)
            {
                BitVector |= 0x02;
            }
            else
            {
                BitVector &= 0xFD;
            }
        }
    }

    public bool IsRestartBackward
    {
        get => (BitVector & 0x04) != 0;
        set
        {
            if (value)
            {
                BitVector |= 0x04;
            }
            else
            {
                BitVector &= 0xFB;
            }
        }
    }

    public bool IsContinuous
    {
        get => (BitVector & 0x08) != 0;
        set
        {
            if (value)
            {
                BitVector |= 0x08;
            }
            else
            {
                BitVector &= 0xF7;
            }
        }
    }

    public bool IsGlobal
    {
        get => (BitVector & 0x10) != 0;
        set
        {
            if (value)
            {
                BitVector |= 0x10;
            }
            else
            {
                BitVector &= 0xEF;
            }
        }
    }

    public Children Children { get; set; }
    public Playlist Playlist { get; set; }

    public bool Read(BinaryReader reader)
    {
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        var loopCount = reader.ReadUInt16();
        var loopModMin = reader.ReadUInt16();
        var loopModMax = reader.ReadUInt16();
        var transitionTime = reader.ReadSingle();
        var transitionTimeModMin = reader.ReadSingle();
        var transitionTimeModMax = reader.ReadSingle();
        var avoidRepeatCount = reader.ReadUInt16();
        var transitionMode = reader.ReadByte();
        var randomMode = reader.ReadByte();
        var mode = reader.ReadByte();
        var bitVector = reader.ReadByte();

        var children = new Children();

        if (!children.Read(reader))
        {
            return false;
        }

        var playlist = new Playlist();

        if (!playlist.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;
        LoopCount = loopCount;
        LoopModMin = loopModMin;
        LoopModMax = loopModMax;
        TransitionTime = transitionTime;
        TransitionTimeModMin = transitionTimeModMin;
        TransitionTimeModMax = transitionTimeModMax;
        AvoidRepeatCount = avoidRepeatCount;
        TransitionMode = transitionMode;
        RandomMode = randomMode;
        Mode = mode;
        BitVector = bitVector;
        Children = children;
        Playlist = playlist;

        return true;
    }
}
