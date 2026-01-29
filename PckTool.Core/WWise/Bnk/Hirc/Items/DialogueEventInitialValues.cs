using PckTool.Core.WWise.Bnk.Hirc.Params;

namespace PckTool.Core.WWise.Bnk.Hirc.Items;

/// <summary>
///     DialogueEvent initial values for bank version 113.
///     Corresponds to CAkDialogueEvent::SetInitialValues in wwiser.
/// </summary>
public class DialogueEventInitialValues
{
    /// <summary>
    ///     Probability (v73+).
    /// </summary>
    public byte Probability { get; set; }

    /// <summary>
    ///     Tree depth (number of arguments).
    /// </summary>
    public uint TreeDepth { get; set; }

    /// <summary>
    ///     Arguments (game syncs).
    /// </summary>
    public List<GameSyncArgument> Arguments { get; set; } = [];

    /// <summary>
    ///     Tree data size in bytes.
    /// </summary>
    public uint TreeDataSize { get; set; }

    /// <summary>
    ///     Decision tree mode (v46+).
    /// </summary>
    public byte TreeMode { get; set; }

    /// <summary>
    ///     Decision tree.
    /// </summary>
    public DecisionTree DecisionTree { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // For v73+ (v113 is in range):
        // uProbability (u8)
        Probability = reader.ReadByte();

        // uTreeDepth (u32)
        TreeDepth = reader.ReadUInt32();

        // Arguments (SetArguments for DialogueEvent)
        // First read all ulGroup IDs, then all eGroupType values
        var groupIds = new uint[TreeDepth];
        var groupTypes = new byte[TreeDepth];

        for (var i = 0; i < TreeDepth; i++)
        {
            groupIds[i] = reader.ReadUInt32();
        }

        for (var i = 0; i < TreeDepth; i++)
        {
            groupTypes[i] = reader.ReadByte();
        }

        for (var i = 0; i < TreeDepth; i++)
        {
            Arguments.Add(new GameSyncArgument { GroupId = groupIds[i], GroupType = groupTypes[i] });
        }

        // uTreeDataSize (u32)
        TreeDataSize = reader.ReadUInt32();

        // uMode (u8) for v46+
        TreeMode = reader.ReadByte();

        // DecisionTree
        var decisionTree = new DecisionTree { Mode = TreeMode };

        if (!decisionTree.Read(reader, TreeDepth, TreeDataSize))
        {
            return false;
        }

        DecisionTree = decisionTree;

        // For v119+, there would be AkPropBundle here
        // But v113 <= 118, so we skip it

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Probability);
        writer.Write(TreeDepth);

        // Write all group IDs first
        foreach (var arg in Arguments)
        {
            writer.Write(arg.GroupId);
        }

        // Then write all group types
        foreach (var arg in Arguments)
        {
            writer.Write(arg.GroupType);
        }

        writer.Write(TreeDataSize);
        writer.Write(TreeMode);
        DecisionTree.Write(writer, TreeDepth);
    }
}
