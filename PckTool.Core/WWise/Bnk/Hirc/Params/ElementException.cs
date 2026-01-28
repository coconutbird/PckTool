namespace PckTool.Core.WWise.Bnk.Hirc.Params;

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

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write((byte) (IsBusId ? 1 : 0));
    }
}
