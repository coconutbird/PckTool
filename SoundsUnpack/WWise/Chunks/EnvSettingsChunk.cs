using SoundsUnpack.WWise.Bank;

namespace SoundsUnpack.WWise.Chunks;

public class EnvSettingsChunk
{
    public ConversionTable ConversionTable { get; set; } = new();

    public bool Read(BinaryReader reader, uint size)
    {
        var conversionTable = new ConversionTable();

        if (!conversionTable.Read(reader, size))
        {
            return false;
        }

        ConversionTable = conversionTable;

        return true;
    }
}