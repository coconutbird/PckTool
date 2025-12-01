namespace SoundsUnpack.WWise.Structs;

public class AuxParams
{
    public byte BitVector { get; set; }

    public bool OverrideUserAuxSends
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

    public bool HasAux
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

    public bool OverrideReflectionsAuxBus
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

    public bool Read(BinaryReader reader)
    {
        BitVector = reader.ReadByte();

        return true;
    }
}