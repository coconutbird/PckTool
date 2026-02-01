using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class BankSourceData
{
    public PluginId PluginId { get; set; }

    /// <summary>
    ///     Gets the plugin type from the lower 4 bits of the plugin ID.
    /// </summary>
    public int PluginTypeValue => (int) ((uint) PluginId & 0x000F);

    public PluginType PluginType => (PluginType) ((uint) PluginId & 0x0000FFFF);
    public PluginCompany PluginCompany => (PluginCompany) (((uint) PluginId & 0xFFFF0000) >> 16);

    public StreamType StreamType { get; set; }
    public MediaInformation MediaInformation { get; set; }

    /// <summary>
    ///     Plugin parameters size. Only present for Source plugins (PluginType == 2) in v113-126.
    /// </summary>
    public uint? PluginParamsSize { get; set; }

    /// <summary>
    ///     Plugin parameters data. Only present if PluginParamsSize > 0.
    /// </summary>
    public byte[]? PluginParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        var pluginId = (PluginId) reader.ReadUInt32();
        var streamType = (StreamType) reader.ReadByte();

        var mediaInformation = new MediaInformation();

        if (!mediaInformation.Read(reader))
        {
            return false;
        }

        // For v113-126, read plugin params for Source plugins (PluginType == 2) or Sink plugins (PluginType == 5)
        // The plugin type is stored in the lower 4 bits of the plugin ID
        // See wwiser CAkBankMgr__LoadSource: if (PluginType == 2 or PluginType == 5): parse_plugin_params
        var pluginTypeValue = (int) ((uint) pluginId & 0x000F);
        uint? pluginParamsSize = null;
        byte[]? pluginParams = null;

        if (pluginTypeValue == 2 || pluginTypeValue == 5) // Source or Sink
        {
            pluginParamsSize = reader.ReadUInt32();

            if (pluginParamsSize > 0)
            {
                pluginParams = reader.ReadBytes((int) pluginParamsSize.Value);
            }
        }

        PluginId = pluginId;
        StreamType = streamType;
        MediaInformation = mediaInformation;
        PluginParamsSize = pluginParamsSize;
        PluginParams = pluginParams;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint) PluginId);
        writer.Write((byte) StreamType);
        MediaInformation.Write(writer);

        // Write plugin params if this is a Source or Sink plugin
        var pluginTypeValue = (int) ((uint) PluginId & 0x000F);

        if (pluginTypeValue == 2 || pluginTypeValue == 5)
        {
            writer.Write(PluginParamsSize ?? 0);

            if (PluginParams != null && PluginParams.Length > 0)
            {
                writer.Write(PluginParams);
            }
        }
    }
}
