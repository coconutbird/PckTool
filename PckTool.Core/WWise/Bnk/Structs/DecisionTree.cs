namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     Decision tree (AkDecisionTree) for MusicSwitch and DialogueEvent.
///     For v73+ (v113 is in range).
/// </summary>
public class DecisionTree
{
    /// <summary>
    ///     Tree mode (AkDecisionTree::Mode).
    /// </summary>
    public byte Mode { get; set; }

    /// <summary>
    ///     Root nodes of the tree.
    /// </summary>
    public List<DecisionTreeNode> RootNodes { get; set; } = [];

    /// <summary>
    ///     Raw tree data (for complex/deeply nested trees).
    /// </summary>
    public byte[]? RawTreeData { get; set; }

    public bool Read(BinaryReader reader, uint depth, uint treeDataSize)
    {
        // For v113: item_size = 0x0c (12 bytes per node)
        const int itemSize = 0x0c;
        var countMax = treeDataSize / itemSize;

        if (countMax == 0)
        {
            return true;
        }

        // Parse tree starting with 1 root node at depth 0
        var startPos = reader.BaseStream.Position;

        try
        {
            ParseTreeNodes(reader, 1, countMax, 0, depth, RootNodes);
        }
        catch
        {
            // If tree parsing fails, fallback to raw data
            reader.BaseStream.Position = startPos;
            RawTreeData = reader.ReadBytes((int) treeDataSize);
        }

        return true;
    }

    private static void ParseTreeNodes(
        BinaryReader reader,
        uint count,
        uint countMax,
        uint curDepth,
        uint maxDepth,
        List<DecisionTreeNode> nodes)
    {
        var nodeInfos = new List<(DecisionTreeNode Node, ushort ChildCount)>();

        for (var i = 0; i < count; i++)
        {
            var node = new DecisionTreeNode { Key = reader.ReadUInt32() };

            // Try to detect if this is an audio node or has children
            // by peeking at the next value
            var nextVal = reader.ReadUInt32();
            var uidx = (ushort) (nextVal & 0xFFFF);
            var ucnt = (ushort) ((nextVal >> 16) & 0xFFFF);
            var isAudioNode = uidx > countMax || ucnt > countMax || curDepth == maxDepth;

            if (isAudioNode)
            {
                node.AudioNodeId = nextVal;
                node.IsAudioNode = true;
                nodeInfos.Add((node, 0));
            }
            else
            {
                node.ChildrenIdx = uidx;
                node.ChildrenCount = ucnt;
                nodeInfos.Add((node, ucnt));
            }

            // Weight and probability (for v46+)
            node.Weight = reader.ReadUInt16();
            node.Probability = reader.ReadUInt16();

            nodes.Add(node);
        }

        // Recursively parse children
        foreach (var (node, childCount) in nodeInfos)
        {
            if (childCount > 0)
            {
                ParseTreeNodes(reader, childCount, countMax, curDepth + 1, maxDepth, node.Children);
            }
        }
    }
}

/// <summary>
///     Decision tree node.
/// </summary>
public class DecisionTreeNode
{
    /// <summary>
    ///     Key (game sync value or 0 for default).
    /// </summary>
    public uint Key { get; set; }

    /// <summary>
    ///     Whether this is an audio/leaf node.
    /// </summary>
    public bool IsAudioNode { get; set; }

    /// <summary>
    ///     Audio node ID (if IsAudioNode).
    /// </summary>
    public uint AudioNodeId { get; set; }

    /// <summary>
    ///     Children start index (if not audio node).
    /// </summary>
    public ushort ChildrenIdx { get; set; }

    /// <summary>
    ///     Children count (if not audio node).
    /// </summary>
    public ushort ChildrenCount { get; set; }

    /// <summary>
    ///     Weight for selection.
    /// </summary>
    public ushort Weight { get; set; }

    /// <summary>
    ///     Probability for selection.
    /// </summary>
    public ushort Probability { get; set; }

    /// <summary>
    ///     Child nodes.
    /// </summary>
    public List<DecisionTreeNode> Children { get; set; } = [];
}
