namespace PckTool.Core.WWise.Bnk.Structs;

public class AdvSettingsParams
{
    public byte BitVector { get; set; }

    public bool KillNewest
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

    public bool UseVirtualBehavior
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

    public bool IgnoreParentMaxNumInst
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

    public bool IsVVoicesOptOverrideParent
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

    public byte VirtualQueueBehavior { get; set; }
    public ushort MaxNumberOfInstances { get; set; }
    public ushort BelowThresholdBehavior { get; set; }

    public byte BitVector2 { get; set; }

    public bool OverrideHdrEnvelope
    {
        get => (BitVector2 & 0x01) != 0;
        set
        {
            if (value)
            {
                BitVector2 |= 0x01;
            }
            else
            {
                BitVector2 &= 0xFE;
            }
        }
    }

    public bool OverrideAnalysis
    {
        get => (BitVector2 & 0x02) != 0;
        set
        {
            if (value)
            {
                BitVector2 |= 0x02;
            }
            else
            {
                BitVector2 &= 0xFD;
            }
        }
    }

    public bool NormalizeLoudness
    {
        get => (BitVector2 & 0x04) != 0;
        set
        {
            if (value)
            {
                BitVector2 |= 0x04;
            }
            else
            {
                BitVector2 &= 0xFB;
            }
        }
    }

    public bool EnableEnvelope
    {
        get => (BitVector2 & 0x08) != 0;
        set
        {
            if (value)
            {
                BitVector2 |= 0x08;
            }
            else
            {
                BitVector2 &= 0xF7;
            }
        }
    }

    public bool Read(BinaryReader reader)
    {
        BitVector = reader.ReadByte();
        VirtualQueueBehavior = reader.ReadByte();
        MaxNumberOfInstances = reader.ReadUInt16();
        BelowThresholdBehavior = reader.ReadByte();
        BitVector2 = reader.ReadByte();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(BitVector);
        writer.Write(VirtualQueueBehavior);
        writer.Write(MaxNumberOfInstances);
        writer.Write((byte) BelowThresholdBehavior);
        writer.Write(BitVector2);
    }
}
