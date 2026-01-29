using PckTool.Core.WWise.Bnk.Hirc.Params;

namespace PckTool.Core.WWise.Bnk.Hirc.Items;

/// <summary>
///     MusicSwitchCntr initial values for bank version 113.
///     Corresponds to CAkMusicSwitchCntr::SetInitialValues in wwiser.
/// </summary>
public class MusicSwitchCntrInitialValues
{
    /// <summary>
    ///     Music transition node params.
    /// </summary>
    public MusicTransNodeParams MusicTransNodeParams { get; set; } = null!;

    /// <summary>
    ///     Continue playback flag.
    /// </summary>
    public byte IsContinuePlayback { get; set; }

    /// <summary>
    ///     Tree depth.
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
    ///     Decision tree mode.
    /// </summary>
    public byte TreeMode { get; set; }

    /// <summary>
    ///     Decision tree.
    /// </summary>
    public DecisionTree DecisionTree { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // MusicTransNodeParams
        var musicTransNodeParams = new MusicTransNodeParams();

        if (!musicTransNodeParams.Read(reader))
        {
            return false;
        }

        MusicTransNodeParams = musicTransNodeParams;

        // For v73+ (v113 is in range):
        // bIsContinuePlayback (u8)
        IsContinuePlayback = reader.ReadByte();

        // uTreeDepth (u32)
        TreeDepth = reader.ReadUInt32();

        // Arguments (SetArguments)
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

        // uMode (u8)
        TreeMode = reader.ReadByte();

        // DecisionTree
        var decisionTree = new DecisionTree { Mode = TreeMode };

        if (!decisionTree.Read(reader, TreeDepth, TreeDataSize))
        {
            return false;
        }

        DecisionTree = decisionTree;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        MusicTransNodeParams.Write(writer);
        writer.Write(IsContinuePlayback);
        writer.Write(TreeDepth);

        // Write group IDs first, then group types (two-pass pattern)
        foreach (var arg in Arguments)
        {
            writer.Write(arg.GroupId);
        }

        foreach (var arg in Arguments)
        {
            writer.Write(arg.GroupType);
        }

        writer.Write(TreeDataSize);
        writer.Write(TreeMode);
        DecisionTree.Write(writer, TreeDepth);
    }
}

/// <summary>
///     Game sync argument (AkGameSync).
/// </summary>
public class GameSyncArgument
{
    /// <summary>
    ///     Group ID (state group or switch group).
    /// </summary>
    public uint GroupId { get; set; }

    /// <summary>
    ///     Group type (AkGroupType: 0=Switch, 1=State).
    /// </summary>
    public byte GroupType { get; set; }
}
