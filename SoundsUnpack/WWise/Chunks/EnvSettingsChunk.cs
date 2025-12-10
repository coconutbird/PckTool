using SoundsUnpack.WWise.Bank;

namespace SoundsUnpack.WWise.Chunks;

public class EnvSettingsChunk : BaseChunk
{
    public override bool IsValid => ConversionTable?.IsValid == true;

    public ConversionTable? ConversionTable { get; private set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
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
