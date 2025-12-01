namespace SoundsUnpack.WWise.Structs;

public class BankSourceData
{
    public uint PluginId { get; set; }
    public byte StreamType { get; set; }
    public MediaInformation MediaInformation { get; set; }

    public bool Read(BinaryReader reader)
    {
        var pluginId = reader.ReadUInt32();
        var streamType = reader.ReadByte();

        var mediaInformation = new MediaInformation();
        if (!mediaInformation.Read(reader))
        {
            return false;
        }

        PluginId = pluginId;
        StreamType = streamType;
        MediaInformation = mediaInformation;

        return true;
    }
}