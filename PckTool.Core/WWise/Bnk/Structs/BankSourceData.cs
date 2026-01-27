using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

public class BankSourceData
{
    public PluginId PluginId { get; set; }
    public PluginType PluginType => (PluginType) ((uint) PluginId & 0x0000FFFF);
    public PluginCompany PluginCompany => (PluginCompany) (((uint) PluginId & 0xFFFF0000) >> 16);

    public StreamType StreamType { get; set; }
    public MediaInformation MediaInformation { get; set; }

    public bool Read(BinaryReader reader)
    {
        var pluginId = (PluginId) reader.ReadUInt32();
        var streamType = (StreamType) reader.ReadByte();

        var mediaInformation = new MediaInformation();

        if (!mediaInformation.Read(reader))
        {
            return false;
        }

        if (pluginId == PluginId.Wwise_Silence)
        {
            var size = reader.ReadUInt32();

            if (size > 0)
            {
                throw new NotImplementedException("Silence plugin with non-zero size is not implemented.");
            }
        }

        PluginId = pluginId;
        StreamType = streamType;
        MediaInformation = mediaInformation;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint) PluginId);
        writer.Write((byte) StreamType);
        MediaInformation.Write(writer);

        if (PluginId == PluginId.Wwise_Silence)
        {
            writer.Write((uint) 0); // size = 0
        }
    }
}
