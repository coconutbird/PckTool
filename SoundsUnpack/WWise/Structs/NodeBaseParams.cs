namespace SoundsUnpack.WWise.Structs;

public class NodeBaseParams
{
    public NodeInitialFxParams NodeInitialFxParams { get; set; }
    
    public bool Read(BinaryReader reader)
    {
        var nodeInitialFxParams = new NodeInitialFxParams();

        if (!nodeInitialFxParams.Read(reader))
        {
            return false;
        }
        
        var overrideAttachmentParams = reader.ReadByte();
        var overrideBusId = reader.ReadUInt32();
        var directParentId = reader.ReadUInt32();
        var byBitVector = reader.ReadByte();

        var nodeInitialParams = new NodeInitialParams();

        return true;
    }
}