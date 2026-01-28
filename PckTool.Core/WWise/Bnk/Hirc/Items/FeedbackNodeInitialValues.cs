using PckTool.Core.WWise.Bnk.Hirc.Params;

namespace PckTool.Core.WWise.Bnk.Hirc.Items;

/// <summary>
///     FeedbackNode initial values for bank version 113.
///     Corresponds to CAkFeedbackNode::SetInitialValues in wwiser.
///     Note: FeedbackNode is deprecated in newer Wwise versions (v125+).
/// </summary>
public class FeedbackNodeInitialValues
{
    /// <summary>
    ///     Feedback sources (AkFeedbackSource).
    /// </summary>
    public List<FeedbackSource> Sources { get; set; } = [];

    /// <summary>
    ///     Node base params (inherited from CAkParameterNodeBase).
    /// </summary>
    public NodeBaseParams NodeBaseParams { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // numSources (u32)
        var numSources = reader.ReadUInt32();

        for (var i = 0; i < numSources; i++)
        {
            var source = new FeedbackSource();

            if (!source.Read(reader))
            {
                return false;
            }

            Sources.Add(source);
        }

        // NodeBaseParams
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint) Sources.Count);

        foreach (var source in Sources)
        {
            source.Write(writer);
        }

        NodeBaseParams.Write(writer);
    }
}

/// <summary>
///     Feedback source (AkFeedbackSource).
/// </summary>
public class FeedbackSource
{
    /// <summary>
    ///     Company ID (device manufacturer).
    /// </summary>
    public ushort CompanyId { get; set; }

    /// <summary>
    ///     Device ID.
    /// </summary>
    public ushort DeviceId { get; set; }

    /// <summary>
    ///     Volume offset in dB.
    /// </summary>
    public float VolumeOffset { get; set; }

    /// <summary>
    ///     Bank source data.
    /// </summary>
    public BankSourceData BankSourceData { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        CompanyId = reader.ReadUInt16();
        DeviceId = reader.ReadUInt16();
        VolumeOffset = reader.ReadSingle();

        var bankSourceData = new BankSourceData();

        if (!bankSourceData.Read(reader))
        {
            return false;
        }

        BankSourceData = bankSourceData;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(CompanyId);
        writer.Write(DeviceId);
        writer.Write(VolumeOffset);
        BankSourceData.Write(writer);
    }
}
