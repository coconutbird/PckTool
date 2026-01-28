namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class PlaylistItem
{
    public int PlayId { get; set; }
    public int Weight { get; set; }

    public bool Read(BinaryReader reader)
    {
        var playId = reader.ReadInt32();
        var weight = reader.ReadInt32();

        PlayId = playId;
        Weight = weight;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(PlayId);
        writer.Write(Weight);
    }
}
