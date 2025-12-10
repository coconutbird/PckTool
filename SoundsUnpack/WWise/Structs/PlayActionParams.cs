namespace SoundsUnpack.WWise.Structs;

public class PlayActionParams
{
    public byte BitVector { get; set; }

    public byte FadeCurve
    {
        get => (byte) (BitVector & 0x0F);
        set => BitVector = (byte) ((BitVector & 0xF0) | (value & 0x0F));
    }

    public uint FileId { get; set; }

    public bool Read(BinaryReader reader)
    {
        var bitVector = reader.ReadByte();
        var fileId = reader.ReadUInt32();

        BitVector = bitVector;
        FileId = fileId;

        return true;
    }
}
