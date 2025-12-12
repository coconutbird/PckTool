namespace PckTool.Core.WWise.Structs;

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
}
