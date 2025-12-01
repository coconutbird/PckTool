namespace SoundsUnpack.WWise.Structs;

public class PositioningParams
{
    public byte BitVector { get; set; }

    public bool PositioningInfoOverrideParent
    {
        get => (BitVector & 0x01) != 0;
        set
        {
            if (value)
                BitVector |= 0x01;
            else
                BitVector &= 0xFE; // 11111110
        }
    }

    public bool Enable2D
    {
        get => (BitVector & 0x02) != 0;
        set
        {
            if (value)
                BitVector |= 0x02;
            else
                BitVector &= 0xFD; // 11111101
        }
    }

    public bool EnableSpatialization
    {
        get => (BitVector & 0x04) != 0;
        set
        {
            if (value)
                BitVector |= 0x04;
            else
                BitVector &= 0xFB; // 11111011
        }
    }

    public bool Is3DPositioningAvailable
    {
        get => (BitVector & 0x08) != 0;
        set
        {
            if (value)
                BitVector |= 0x08;
            else
                BitVector &= 0xF7;
        }
    }

    public byte? Bits3D { get; set; }

    public byte SpatializationMode
    {
        get => (byte)(Bits3D & 0x03)!;
        set
        {
            Is3DPositioningAvailable = true;
            Bits3D ??= 0;
            Bits3D &= 0xFC;
            Bits3D |= (byte)(value & 0x03);
        }
    }

    public bool HoldEmitterPosAndOrient
    {
        get => (Bits3D & 0x08) != 0;
        set
        {
            Is3DPositioningAvailable = true;
            Bits3D ??= 0;
            if (value)
                Bits3D |= 0x08;
            else
                Bits3D &= 0xF7;
        }
    }

    public bool HoldListenerOrient
    {
        get => (Bits3D & 0x10) != 0;
        set
        {
            Is3DPositioningAvailable = true;
            Bits3D ??= 0;
            if (value)
                Bits3D |= 0x10;
            else
                Bits3D &= 0xEF;
        }
    }

    public uint? AttenuationId { get; set; }

    public bool Read(BinaryReader reader)
    {
        BitVector = reader.ReadByte();

        if (Is3DPositioningAvailable)
        {
            Bits3D = reader.ReadByte();

            // TODO: verify if AttenuationId is always present when 3D positioning is available
            AttenuationId = reader.ReadUInt32();
        }

        return true;
    }
}