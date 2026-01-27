namespace PckTool.Core.WWise.Bnk.Structs;

public class EventInitialValues
{
    public List<uint> Actions { get; set; } = new();

    public bool Read(BinaryReader reader)
    {
        var actionListSize = reader.ReadUInt32();

        for (var i = 0; i < actionListSize; ++i)
        {
            Actions.Add(reader.ReadUInt32());
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint) Actions.Count);

        foreach (var action in Actions)
        {
            writer.Write(action);
        }
    }
}
