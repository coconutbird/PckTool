namespace SoundsUnpack.WWise.Structs;

public class ResumeActionSpecificParams
{
    public byte BitVector { get; set; }

    public bool IsMasterResume
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
                BitVector &= 0xFE; // 11111110
            }
        }
    }

    public bool ApplyToStateTransitions
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
                BitVector &= 0xFD; // 11111101
            }
        }
    }

    public bool ApplyToDynamicSequence
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
                BitVector &= 0xFB; // 11111011
            }
        }
    }

    public bool Read(BinaryReader reader)
    {
        var bitVector = reader.ReadByte();

        BitVector = bitVector;

        return true;
    }
}
