namespace SoundsUnpack.WWise.Structs;

public class Children
{
    public List<int> ChildIds { get; set; } = new();

    public bool Read(BinaryReader reader)
    {
        var numberOfChildren = reader.ReadUInt32();

        for (var i = 0; i < numberOfChildren; i++)
        {
            ChildIds.Add(reader.ReadInt32());
        }

        return true;
    }
}