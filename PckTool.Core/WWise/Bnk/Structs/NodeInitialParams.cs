namespace PckTool.Core.WWise.Bnk.Structs;

public class NodeInitialParams
{
    public PropBundle PropBundle1 { get; set; } = null!;
    public PropBundle PropBundle2 { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        var propBundle1 = new PropBundle();

        if (!propBundle1.Read(reader))
        {
            return false;
        }

        var propBundle2 = new PropBundle();

        if (!propBundle2.Read(reader, true))
        {
            return false;
        }

        PropBundle1 = propBundle1;
        PropBundle2 = propBundle2;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        PropBundle1.Write(writer);
        PropBundle2.Write(writer);
    }
}
