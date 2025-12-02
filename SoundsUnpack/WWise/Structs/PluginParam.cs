namespace SoundsUnpack.WWise.Structs;

public class PluginParam
{
    public byte[] ParamBlock { get; set; } = [];

    public bool Read(BinaryReader reader, uint size)
    {
        var paramBlock = reader.ReadBytes((int)size);

        ParamBlock = paramBlock;

        return true;
    }
}