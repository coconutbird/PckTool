namespace SoundsUnpack.WWise.Structs;

public class PlayActionParams
{
    public byte BitVector { get; set; }

    /// <summary>
    ///     Fade curve type. Uses 5 bits (0x1F mask) per wwiser.
    /// </summary>
    public byte FadeCurve
    {
        get => (byte) (BitVector & 0x1F);
        set => BitVector = (byte) ((BitVector & 0xE0) | (value & 0x1F));
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
