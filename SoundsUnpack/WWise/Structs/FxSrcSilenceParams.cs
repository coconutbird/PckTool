namespace SoundsUnpack.WWise.Structs;

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
}