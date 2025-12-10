namespace SoundsUnpack.WWise.Structs;

public class SoundInitialValues
{
    public BankSourceData BankSourceData { get; set; }
    public NodeBaseParams NodeBaseParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        var bankSourceData = new BankSourceData();

        if (!bankSourceData.Read(reader))
        {
            return false;
        }

        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        BankSourceData = bankSourceData;
        NodeBaseParams = nodeBaseParams;

        return true;
    }
}
