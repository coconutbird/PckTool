namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class FxSrcSilenceParams
{
    public float Duration { get; set; }
    public float RandomizedLengthMinus { get; set; }
    public float RandomizedLengthPlus { get; set; }

    public bool Read(BinaryReader reader)
    {
        Duration = reader.ReadSingle();
        RandomizedLengthMinus = reader.ReadSingle();
        RandomizedLengthPlus = reader.ReadSingle();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Duration);
        writer.Write(RandomizedLengthMinus);
        writer.Write(RandomizedLengthPlus);
    }
}
