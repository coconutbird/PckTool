namespace SoundsUnpack.WWise.Structs;

public class ElementException
{
    public uint Id { get; set; }
    public bool IsBusId { get; set; }

    public bool Read(BinaryReader reader)
    {
        Id = reader.ReadUInt32();
        IsBusId = reader.ReadByte() != 0;

        return true;
    }
}