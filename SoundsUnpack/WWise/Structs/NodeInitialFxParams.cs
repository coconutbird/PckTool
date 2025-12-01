namespace SoundsUnpack.WWise.Structs;

public class NodeInitialFxParams
{
    public byte IsOverrideParentFx { get; set; }
    public byte NumFx { get; set; }
    
    public bool Read(BinaryReader reader)
    {
        IsOverrideParentFx = reader.ReadByte();
        NumFx = reader.ReadByte();
        
        return true;
    }
}