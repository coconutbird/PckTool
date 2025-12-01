namespace SoundsUnpack.WWise.Structs;

public class BankSourceData
{
    public uint PluginId { get; set; }
    public byte StreamType { get; set; }
    public MediaInformation MediaInformation { get; set; }

    public bool Read(BinaryReader reader)
    {
        PluginId = reader.ReadUInt32();
        StreamType = reader.ReadByte();

        MediaInformation = new MediaInformation();

        return MediaInformation.Read(reader);
    }
}