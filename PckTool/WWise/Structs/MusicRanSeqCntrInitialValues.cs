namespace PckTool.WWise.Structs;

/// <summary>
///     MusicRanSeqCntr initial values for bank version 113.
///     Corresponds to CAkMusicRanSeqCntr::SetInitialValues in wwiser.
/// </summary>
public class MusicRanSeqCntrInitialValues
{
    /// <summary>
    ///     Music transition node params.
    /// </summary>
    public MusicTransNodeParams MusicTransNodeParams { get; set; } = null!;

    /// <summary>
    ///     Number of playlist items at root level.
    /// </summary>
    public uint NumPlaylistItems { get; set; }

    /// <summary>
    ///     Root playlist items (recursive structure).
    /// </summary>
    public List<MusicRanSeqPlaylistItem> Playlist { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // MusicTransNodeParams
        var musicTransNodeParams = new MusicTransNodeParams();

        if (!musicTransNodeParams.Read(reader))
        {
            return false;
        }

        MusicTransNodeParams = musicTransNodeParams;

        // numPlaylistItems (u32)
        NumPlaylistItems = reader.ReadUInt32();

        // Parse playlist recursively starting with 1 root item
        ParsePlaylistNodes(reader, 1, Playlist);

        return true;
    }

    private static void ParsePlaylistNodes(BinaryReader reader, uint count, List<MusicRanSeqPlaylistItem> items)
    {
        for (var i = 0; i < count; i++)
        {
            var item = new MusicRanSeqPlaylistItem
            {
                SegmentId = reader.ReadUInt32(),
                PlaylistItemId = reader.ReadUInt32(),
                NumChildren = reader.ReadUInt32()
            };

            // For v113 (v46+):
            // eRSType, Loop, LoopMin, LoopMax, Weight, wAvoidRepeatCount, bIsUsingWeight, bIsShuffle
            item.RsType = reader.ReadUInt32();
            item.Loop = reader.ReadInt16();
            item.LoopMin = reader.ReadInt16();
            item.LoopMax = reader.ReadInt16();
            item.Weight = reader.ReadUInt32();
            item.AvoidRepeatCount = reader.ReadUInt16();
            item.IsUsingWeight = reader.ReadByte();
            item.IsShuffle = reader.ReadByte();

            // Recursively parse children
            if (item.NumChildren > 0)
            {
                ParsePlaylistNodes(reader, item.NumChildren, item.Children);
            }

            items.Add(item);
        }
    }
}

/// <summary>
///     Music random/sequence playlist item (AkMusicRanSeqPlaylistItem).
/// </summary>
public class MusicRanSeqPlaylistItem
{
    /// <summary>
    ///     Segment ID to play (0 if this is a group node).
    /// </summary>
    public uint SegmentId { get; set; }

    /// <summary>
    ///     Playlist item ID.
    /// </summary>
    public uint PlaylistItemId { get; set; }

    /// <summary>
    ///     Number of child items.
    /// </summary>
    public uint NumChildren { get; set; }

    /// <summary>
    ///     Random/sequence type (RSType enum).
    /// </summary>
    public uint RsType { get; set; }

    /// <summary>
    ///     Loop count (-1 = infinite).
    /// </summary>
    public short Loop { get; set; }

    /// <summary>
    ///     Loop min (v90+).
    /// </summary>
    public short LoopMin { get; set; }

    /// <summary>
    ///     Loop max (v90+).
    /// </summary>
    public short LoopMax { get; set; }

    /// <summary>
    ///     Weight for random selection.
    /// </summary>
    public uint Weight { get; set; }

    /// <summary>
    ///     Avoid repeat count.
    /// </summary>
    public ushort AvoidRepeatCount { get; set; }

    /// <summary>
    ///     Whether weight is used for selection.
    /// </summary>
    public byte IsUsingWeight { get; set; }

    /// <summary>
    ///     Whether shuffle mode is enabled.
    /// </summary>
    public byte IsShuffle { get; set; }

    /// <summary>
    ///     Child playlist items.
    /// </summary>
    public List<MusicRanSeqPlaylistItem> Children { get; set; } = [];
}
