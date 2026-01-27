namespace PckTool.Core.WWise.Bnk.Structs;

public class Children
{
    public List<uint> ChildIds { get; set; } = new();

    public bool Read(BinaryReader reader)
    {
        var numberOfChildren = reader.ReadUInt32();

        for (var i = 0; i < numberOfChildren; i++)
        {
            ChildIds.Add(reader.ReadUInt32());
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint) ChildIds.Count);

        foreach (var childId in ChildIds)
        {
            writer.Write(childId);
        }
    }
}
