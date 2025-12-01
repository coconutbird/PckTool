namespace SoundsUnpack.WWise.Structs;

public class ActiveActionParams
{
    public byte BitVector { get; set; }
    
    public byte FadeCurve
    {
        get => (byte)(BitVector & 0x0F);
        set => BitVector = (byte)((BitVector & 0xF0) | (value & 0x0F));
    }
    
    public ExceptParams ExceptParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        var bitVector = reader.ReadByte();

        var exceptParams = new ExceptParams();
        if (!exceptParams.Read(reader))
        {
            return false;
        }

        BitVector = bitVector;
        ExceptParams = exceptParams;

        return true;
    }
}