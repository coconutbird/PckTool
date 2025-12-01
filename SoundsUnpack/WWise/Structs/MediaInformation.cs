namespace SoundsUnpack.WWise.Structs;

public class MediaInformation
{
    public int SourceId { get; set; }
    public uint InMemoryMediaSize { get; set; }
    public byte SourceBits { get; set; }

    public bool Read(BinaryReader reader)
    {
        var sourceId = reader.ReadInt32();
        var inMemoryMediaSize = reader.ReadUInt32();
        var sourceBits = reader.ReadByte();

        SourceId = sourceId;
        InMemoryMediaSize = inMemoryMediaSize;
        SourceBits = sourceBits;

        return true;
    }
}