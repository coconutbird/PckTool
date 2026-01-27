namespace PckTool.Core.WWise.Bnk.Structs;

public class Ak3DAutomationParams
{
    public float XRange { get; set; }
    public float YRange { get; set; }
    public float ZRange { get; set; }

    public bool Read(BinaryReader reader)
    {
        XRange = reader.ReadSingle();
        YRange = reader.ReadSingle();
        ZRange = reader.ReadSingle();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(XRange);
        writer.Write(YRange);
        writer.Write(ZRange);
    }
}
