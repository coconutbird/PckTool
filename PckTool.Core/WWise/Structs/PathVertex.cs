namespace PckTool.Core.WWise.Structs;

public class PathVertex
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public int Duration { get; set; }

    public bool Read(BinaryReader reader)
    {
        X = reader.ReadSingle();
        Y = reader.ReadSingle();
        Z = reader.ReadSingle();
        Duration = reader.ReadInt32();

        return true;
    }
}
