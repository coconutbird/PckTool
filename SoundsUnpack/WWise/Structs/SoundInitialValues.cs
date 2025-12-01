namespace SoundsUnpack.WWise.Structs;

public class SoundInitialValues
{
    public BankSourceData BankSourceData { get; set; }
    public NodeBaseParams NodeBaseParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        return false;
    }
}