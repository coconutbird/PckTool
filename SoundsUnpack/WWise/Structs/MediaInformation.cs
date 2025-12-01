namespace SoundsUnpack.WWise.Structs;

public class MediaInformation
{
    public uint SourceId { get; set; }
    public uint InMemoryMediaSize { get; set; }
    public byte SourceBits { get; set; }
    
    public bool Read(BinaryReader reader)
    {
        SourceId = reader.ReadUInt32();
        InMemoryMediaSize = reader.ReadUInt32();
        SourceBits = reader.ReadByte();

        return true;
    }
}