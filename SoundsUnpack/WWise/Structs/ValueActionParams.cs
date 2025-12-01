namespace SoundsUnpack.WWise.Structs;

public class ValueActionParams
{
    public byte BitVector { get; set; }

    public byte FadeCurve
    {
        get => (byte)(BitVector & 0x0F);
        set => BitVector = (byte)((BitVector & 0xF0) | (value & 0x0F));
    }

    public PropActionSpecificParams PropActionSpecificParams { get; set; }
    public ExceptParams ExceptParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        var bitVector = reader.ReadByte();

        var propActionSpecificParams = new PropActionSpecificParams();
        if (!propActionSpecificParams.Read(reader))
        {
            return false;
        }

        var exceptParams = new ExceptParams();
        if (!exceptParams.Read(reader))
        {
            return false;
        }

        BitVector = bitVector;
        PropActionSpecificParams = propActionSpecificParams;
        ExceptParams = exceptParams;

        return true;
    }
}