namespace PckTool.Core.WWise.Bnk.Structs;

public class RandomizerModifier
{
    public float Base { get; set; }
    public float Min { get; set; }
    public float Max { get; set; }

    public bool Read(BinaryReader reader)
    {
        Base = reader.ReadSingle();
        Min = reader.ReadSingle();
        Max = reader.ReadSingle();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Base);
        writer.Write(Min);
        writer.Write(Max);
    }
}
