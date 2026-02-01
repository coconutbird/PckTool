namespace PckTool.Core.WWise.Bnk.Hirc.Params;

/// <summary>
///     Represents the AkPathListItemOffset structure used in 3D positioning automation.
///     This is different from PlaylistItem which is used in playlist containers.
/// </summary>
public class PathListItemOffset
{
    /// <summary>
    ///     Offset into the vertices array.
    /// </summary>
    public uint VerticesOffset { get; set; }

    /// <summary>
    ///     Number of vertices for this path segment.
    /// </summary>
    public uint NumVertices { get; set; }

    public bool Read(BinaryReader reader)
    {
        VerticesOffset = reader.ReadUInt32();
        NumVertices = reader.ReadUInt32();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(VerticesOffset);
        writer.Write(NumVertices);
    }
}
