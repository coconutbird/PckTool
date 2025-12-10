namespace SoundsUnpack.WWise.Structs;

public class ExceptParams
{
    public List<ElementException> ElementExceptions { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        var exceptionListSize = reader.ReadUInt32();

        for (var i = 0; i < exceptionListSize; ++i)
        {
            var listElementException = new ElementException();

            if (!listElementException.Read(reader))
            {
                return false;
            }

            ElementExceptions.Add(listElementException);
        }

        return true;
    }
}
