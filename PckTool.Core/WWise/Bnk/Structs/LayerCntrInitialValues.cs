namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     LayerCntr initial values for bank version 113.
///     Corresponds to CAkLayerCntr::SetInitialValues in wwiser.
/// </summary>
public class LayerCntrInitialValues
{
    /// <summary>
    ///     Node base parameters.
    /// </summary>
    public NodeBaseParams NodeBaseParams { get; set; } = null!;

    /// <summary>
    ///     Child node IDs.
    /// </summary>
    public Children Children { get; set; } = null!;

    /// <summary>
    ///     Layer definitions.
    /// </summary>
    public List<LayerInitialValues> Layers { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // NodeBaseParams
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;

        // Children
        var children = new Children();

        if (!children.Read(reader))
        {
            return false;
        }

        Children = children;

        // ulNumLayers (u32)
        var numLayers = reader.ReadUInt32();

        for (var i = 0; i < numLayers; i++)
        {
            var layer = new LayerInitialValues();

            if (!layer.Read(reader))
            {
                return false;
            }

            Layers.Add(layer);
        }

        // For v119+, there would be bIsContinuousValidation here
        // But v113 <= 118, so we skip it

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        NodeBaseParams.Write(writer);
        Children.Write(writer);

        writer.Write((uint) Layers.Count);

        foreach (var layer in Layers)
        {
            layer.Write(writer);
        }
    }
}

/// <summary>
///     Layer initial values.
///     Corresponds to CAkLayer::SetInitialValues in wwiser.
/// </summary>
public class LayerInitialValues
{
    /// <summary>
    ///     Layer ID.
    /// </summary>
    public uint LayerId { get; set; }

    /// <summary>
    ///     Initial RTPC values for this layer.
    /// </summary>
    public InitialRtpc InitialRtpc { get; set; } = null!;

    /// <summary>
    ///     RTPC ID for crossfading.
    /// </summary>
    public uint RtpcId { get; set; }

    /// <summary>
    ///     RTPC type (v90+).
    /// </summary>
    public byte RtpcType { get; set; }

    /// <summary>
    ///     Associated child data for crossfading.
    /// </summary>
    public List<AssociatedChildData> Associations { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // ulLayerID (tid)
        LayerId = reader.ReadUInt32();

        // SetInitialRTPC (same as CAkParameterNodeBase)
        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        InitialRtpc = initialRtpc;

        // rtpcID (tid)
        RtpcId = reader.ReadUInt32();

        // rtpcType (u8) for v90+
        RtpcType = reader.ReadByte();

        // ulNumAssoc (u32)
        var numAssoc = reader.ReadUInt32();

        for (var i = 0; i < numAssoc; i++)
        {
            var assoc = new AssociatedChildData();

            if (!assoc.Read(reader))
            {
                return false;
            }

            Associations.Add(assoc);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(LayerId);
        InitialRtpc.Write(writer);
        writer.Write(RtpcId);
        writer.Write(RtpcType);

        writer.Write((uint) Associations.Count);

        foreach (var assoc in Associations)
        {
            assoc.Write(writer);
        }
    }
}

/// <summary>
///     Associated child data for layer crossfading.
/// </summary>
public class AssociatedChildData
{
    public uint AssociatedChildId { get; set; }
    public List<RtpcGraphPointBase<float>> Curve { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        AssociatedChildId = reader.ReadUInt32();
        var curveSize = reader.ReadUInt32();

        for (var i = 0; i < curveSize; i++)
        {
            var point = new RtpcGraphPointBase<float>();

            if (!point.Read(reader))
            {
                return false;
            }

            Curve.Add(point);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(AssociatedChildId);
        writer.Write((uint) Curve.Count);

        foreach (var point in Curve)
        {
            point.Write(writer);
        }
    }
}
