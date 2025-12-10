using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Structs;

public class FxBaseInitialValues
{
    public PluginId FxId { get; set; }
    public uint Size { get; set; }
    public PluginParam? PluginParam { get; set; }
    public FxSrcSilenceParams? FxSrcSilenceParams { get; set; }
    public DelayFxParams? DelayFxParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        var fxId = (PluginId) reader.ReadUInt32();
        var size = reader.ReadUInt32();

        PluginParam? pluginParam = null;
        FxSrcSilenceParams? fxSrcSilenceParams = null;
        DelayFxParams? delayFxParams = null;

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
                throw new NotImplementedException($"FxBaseInitialValues for FxId {fxId} is not implemented.");
        }

        var numberOfBankData = reader.ReadByte();

        if (numberOfBankData > 0)
        {
            throw new NotImplementedException("FxBaseInitialValues with Bank Data is not implemented.");
        }

        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        var numberInit = reader.ReadUInt16();

        if (numberInit > 0)
        {
            throw new NotImplementedException("FxBaseInitialValues with Init Parameters is not implemented.");
        }

        FxId = fxId;
        Size = size;

        PluginParam = pluginParam;
        FxSrcSilenceParams = fxSrcSilenceParams;
        DelayFxParams = delayFxParams;

        return true;
    }
}
