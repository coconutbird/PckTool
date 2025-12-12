namespace PckTool.Core.WWise.Bnk.Structs;

public class NodeBaseParams
{
    public NodeInitialFxParams NodeInitialFxParams { get; set; }
    public byte OverrideAttachmentParams { get; set; }
    public uint OverrideBusId { get; set; }
    public uint DirectParentId { get; set; }
    public byte ByBitVector { get; set; }

    public bool PriorityOverrideParent
    {
        get => (ByBitVector & 0x01) != 0;
        set
        {
            if (value)
            {
                ByBitVector |= 0x01;
            }
            else
            {
                ByBitVector &= 0xFE;
            }
        }
    }

    public bool PriorityApplyDistFactor
    {
        get => (ByBitVector & 0x02) != 0;
        set
        {
            if (value)
            {
                ByBitVector |= 0x02;
            }
            else
            {
                ByBitVector &= 0xFD;
            }
        }
    }

    public bool OverrideMidiEventsBehavior
    {
        get => (ByBitVector & 0x04) != 0;
        set
        {
            if (value)
            {
                ByBitVector |= 0x04;
            }
            else
            {
                ByBitVector &= 0xFB;
            }
        }
    }

    public bool OverrideMidiNoteTracking
    {
        get => (ByBitVector & 0x08) != 0;
        set
        {
            if (value)
            {
                ByBitVector |= 0x08;
            }
            else
            {
                ByBitVector &= 0xF7;
            }
        }
    }

    public bool EnableMidiNoteTracking
    {
        get => (ByBitVector & 0x10) != 0;
        set
        {
            if (value)
            {
                ByBitVector |= 0x10;
            }
            else
            {
                ByBitVector &= 0xEF;
            }
        }
    }

    public bool IsMidiBreakLoopOnNoteOff
    {
        get => (ByBitVector & 0x20) != 0;
        set
        {
            if (value)
            {
                ByBitVector |= 0x20;
            }
            else
            {
                ByBitVector &= 0xDF;
            }
        }
    }

    public NodeInitialParams NodeInitialParams { get; set; }
    public PositioningParams PositioningParams { get; set; }
    public AuxParams AuxParams { get; set; }
    public AdvSettingsParams AdvSettingsParams { get; set; }
    public StateChunk StateChunk { get; set; }
    public InitialRtpc InitialRtpc { get; set; }

    public bool Read(BinaryReader reader)
    {
        var nodeInitialFxParams = new NodeInitialFxParams();

        if (!nodeInitialFxParams.Read(reader))
        {
            return false;
        }

        var overrideAttachmentParams = reader.ReadByte();
        var overrideBusId = reader.ReadUInt32();
        var directParentId = reader.ReadUInt32();
        var bitVector = reader.ReadByte();

        var nodeInitialParams = new NodeInitialParams();

        if (!nodeInitialParams.Read(reader))
        {
            return false;
        }

        var positioningParams = new PositioningParams();

        if (!positioningParams.Read(reader))
        {
            return false;
        }

        var auxParams = new AuxParams();

        if (!auxParams.Read(reader))
        {
            return false;
        }

        var advSettingsParams = new AdvSettingsParams();

        if (!advSettingsParams.Read(reader))
        {
            return false;
        }

        var stateChunk = new StateChunk();

        if (!stateChunk.Read(reader))
        {
            return false;
        }

        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        NodeInitialFxParams = nodeInitialFxParams;

        OverrideAttachmentParams = overrideAttachmentParams;
        OverrideBusId = overrideBusId;
        DirectParentId = directParentId;
        ByBitVector = bitVector;

        NodeInitialParams = nodeInitialParams;
        PositioningParams = positioningParams;
        AuxParams = auxParams;
        AdvSettingsParams = advSettingsParams;
        StateChunk = stateChunk;
        InitialRtpc = initialRtpc;

        return true;
    }
}
