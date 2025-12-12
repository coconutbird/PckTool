namespace PckTool.Core.WWise.Bnk.Structs;

public class MediaHeader
{
    public const int SizeOf = 12;
    public uint Id { get; set; }
    public uint Offset { get; set; }
    public uint Size { get; set; }

    public bool Read(BinaryReader reader)
    {
        if (reader.BaseStream.Position + SizeOf > reader.BaseStream.Length)
        {
            return false;
        }

        Id = reader.ReadUInt32();
        Offset = reader.ReadUInt32();
        Size = reader.ReadUInt32();

        return true;
    }
}
