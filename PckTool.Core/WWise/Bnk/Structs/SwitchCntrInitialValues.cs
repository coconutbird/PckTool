namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     SwitchCntr initial values for bank version 113.
///     Corresponds to CAkSwitchCntr::SetInitialValues in wwiser.
/// </summary>
public class SwitchCntrInitialValues
{
    /// <summary>
    ///     Node base parameters.
    /// </summary>
    public NodeBaseParams NodeBaseParams { get; set; } = null!;

    /// <summary>
    ///     Group type (0=Switch, 1=State). For v90+ this is a byte, for earlier versions it was uint32.
    /// </summary>
    public byte GroupType { get; set; }

    /// <summary>
    ///     Switch group ID or state group ID.
    /// </summary>
    public uint GroupId { get; set; }

    /// <summary>
    ///     Default switch or state value.
    /// </summary>
    public uint DefaultSwitch { get; set; }

    /// <summary>
    ///     Continuous validation flag.
    /// </summary>
    public byte IsContinuousValidation { get; set; }

    /// <summary>
    ///     Child node IDs.
    /// </summary>
    public Children Children { get; set; } = null!;

    /// <summary>
    ///     Switch packages mapping switch values to child nodes.
    /// </summary>
    public List<SwitchPackage> SwitchList { get; set; } = [];

    /// <summary>
    ///     Switch node parameters for each child.
    /// </summary>
    public List<SwitchNodeParams> SwitchParams { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // NodeBaseParams
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;

        // eGroupType (u8 for v90+)
        GroupType = reader.ReadByte();

        // ulGroupID (tid)
        GroupId = reader.ReadUInt32();

        // ulDefaultSwitch (tid)
        DefaultSwitch = reader.ReadUInt32();

        // bIsContinuousValidation (u8)
        IsContinuousValidation = reader.ReadByte();

        // Children
        var children = new Children();

        if (!children.Read(reader))
        {
            return false;
        }

        Children = children;

        // ulNumSwitchGroups (u32)
        var numSwitchGroups = reader.ReadUInt32();

        for (var i = 0; i < numSwitchGroups; i++)
        {
            var package = new SwitchPackage();

            if (!package.Read(reader))
            {
                return false;
            }

            SwitchList.Add(package);
        }

        // ulNumSwitchParams (u32)
        var numSwitchParams = reader.ReadUInt32();

        for (var i = 0; i < numSwitchParams; i++)
        {
            var param = new SwitchNodeParams();

            if (!param.Read(reader))
            {
                return false;
            }

            SwitchParams.Add(param);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        NodeBaseParams.Write(writer);
        writer.Write(GroupType);
        writer.Write(GroupId);
        writer.Write(DefaultSwitch);
        writer.Write(IsContinuousValidation);
        Children.Write(writer);

        writer.Write((uint) SwitchList.Count);

        foreach (var package in SwitchList)
        {
            package.Write(writer);
        }

        writer.Write((uint) SwitchParams.Count);

        foreach (var param in SwitchParams)
        {
            param.Write(writer);
        }
    }
}

/// <summary>
///     A switch package mapping a switch value to child nodes.
/// </summary>
public class SwitchPackage
{
    /// <summary>
    ///     Switch or state value ID.
    /// </summary>
    public uint SwitchId { get; set; }

    /// <summary>
    ///     Child node IDs for this switch value.
    /// </summary>
    public List<uint> NodeIds { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        SwitchId = reader.ReadUInt32();
        var numItems = reader.ReadUInt32();

        for (var i = 0; i < numItems; i++)
        {
            NodeIds.Add(reader.ReadUInt32());
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(SwitchId);
        writer.Write((uint) NodeIds.Count);

        foreach (var nodeId in NodeIds)
        {
            writer.Write(nodeId);
        }
    }
}

/// <summary>
///     Parameters for a switch child node.
/// </summary>
public class SwitchNodeParams
{
    public uint NodeId { get; set; }
    public byte BitVector { get; set; }
    public byte ModeBitVector { get; set; }
    public int FadeOutTime { get; set; }
    public int FadeInTime { get; set; }

    public bool IsFirstOnly => (BitVector & 0x01) != 0;
    public bool ContinuePlayback => (BitVector & 0x02) != 0;

    public bool Read(BinaryReader reader)
    {
        NodeId = reader.ReadUInt32();

        // For v90-150: byBitVector contains bIsFirstOnly and bContinuePlayback
        // For v90-150: another byBitVector contains eOnSwitchMode
        BitVector = reader.ReadByte();
        ModeBitVector = reader.ReadByte();
        FadeOutTime = reader.ReadInt32();
        FadeInTime = reader.ReadInt32();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(NodeId);
        writer.Write(BitVector);
        writer.Write(ModeBitVector);
        writer.Write(FadeOutTime);
        writer.Write(FadeInTime);
    }
}
