namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class Playlist
{
    public List<PlaylistItem> Items { get; set; } = new();

    public bool Read(BinaryReader reader)
    {
        Items.Clear();

        var numberOfItems = reader.ReadUInt16();

        for (var i = 0; i < numberOfItems; ++i)
        {
            var item = new PlaylistItem();

            if (!item.Read(reader))
            {
                return false;
            }

            Items.Add(item);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((ushort) Items.Count);

        foreach (var item in Items)
        {
            item.Write(writer);
        }
    }
}
