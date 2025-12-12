using PckTool.WWise.Enums;

namespace PckTool.WWise.Structs;

/// <summary>
///     FxBase initial values for bank version 113.
///     Corresponds to CAkFxBase::SetInitialValues in wwiser.
///     Used by both FxShareSet and FxCustom HIRC types.
/// </summary>
public class FxBaseInitialValues
{
    public PluginId FxId { get; set; }
    public uint Size { get; set; }
    public PluginParam? PluginParam { get; set; }
    public FxSrcSilenceParams? FxSrcSilenceParams { get; set; }
    public DelayFxParams? DelayFxParams { get; set; }
    public byte[]? RawPluginData { get; set; }
    public List<MediaMapEntry> MediaMap { get; set; } = [];
    public InitialRtpc InitialRtpc { get; set; } = null!;
    public List<RtpcInit> RtpcInitList { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // See CAkFxBase__SetInitialValues in wparser.py

        // 1. fxID (plugin ID)
        var fxId = (PluginId) reader.ReadUInt32();

        // 2. Plugin params (uSize + pParamBlock)
        var size = reader.ReadUInt32();

        PluginParam? pluginParam = null;
        FxSrcSilenceParams? fxSrcSilenceParams = null;
        DelayFxParams? delayFxParams = null;
        byte[]? rawPluginData = null;

        if (size > 0)
        {
            switch (fxId)
            {
                case PluginId.Wwise_Compressor:
                {
                    pluginParam = new PluginParam();

                    if (!pluginParam.Read(reader, size))
                    {
                        return false;
                    }

                    break;
                }

                case PluginId.Wwise_Silence:
                {
                    fxSrcSilenceParams = new FxSrcSilenceParams();

                    if (!fxSrcSilenceParams.Read(reader))
                    {
                        return false;
                    }

                    break;
                }

                case PluginId.Wwise_Delay:
                {
                    delayFxParams = new DelayFxParams();

                    if (!delayFxParams.Read(reader))
                    {
                        return false;
                    }

                    break;
                }

                default:
                    // Unknown plugin - skip the data
                    rawPluginData = reader.ReadBytes((int) size);

                    break;
            }
        }

        // 3. uNumBankData + media list (AkMediaMap)
        var numBankData = reader.ReadByte();
        var mediaMap = new List<MediaMapEntry>();

        for (var i = 0; i < numBankData; i++)
        {
            var entry = new MediaMapEntry { Index = reader.ReadByte(), SourceId = reader.ReadUInt32() };
            mediaMap.Add(entry);
        }

        // 4. InitialRTPC
        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        // 5. For v90-126: ulNumInit + rtpc init list
        // For v113, ParamID is u8 (v<=113 uses u8, v114+ uses var)
        var numInit = reader.ReadUInt16();
        var rtpcInitList = new List<RtpcInit>();

        for (var i = 0; i < numInit; i++)
        {
            var init = new RtpcInit
            {
                ParamId = reader.ReadByte(), // u8 for v<=113
                InitValue = reader.ReadSingle()
            };

            rtpcInitList.Add(init);
        }

        FxId = fxId;
        Size = size;
        PluginParam = pluginParam;
        FxSrcSilenceParams = fxSrcSilenceParams;
        DelayFxParams = delayFxParams;
        RawPluginData = rawPluginData;
        MediaMap = mediaMap;
        InitialRtpc = initialRtpc;
        RtpcInitList = rtpcInitList;

        return true;
    }
}

/// <summary>
///     Media map entry (AkMediaMap).
/// </summary>
public class MediaMapEntry
{
    public byte Index { get; set; }
    public uint SourceId { get; set; }
}

/// <summary>
///     RTPC init entry (RTPCInit).
/// </summary>
public class RtpcInit
{
    public byte ParamId { get; set; } // u8 for v<=113, var for v114+
    public float InitValue { get; set; }
}
