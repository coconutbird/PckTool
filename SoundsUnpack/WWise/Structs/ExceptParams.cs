namespace SoundsUnpack.WWise.Structs;

public class ExceptParams
{
    public bool Read(BinaryReader reader)
    {
        var exceptionListSize = reader.ReadUInt32();
        if (exceptionListSize > 0)
        {
            throw new NotImplementedException("ExceptParams is not implemented.");
        }

        return true;
    }
}