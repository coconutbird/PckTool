using PckTool.Core.WWise.Common;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class EnvSettingsChunk : BaseChunk
{
    public override bool IsValid => ConversionTable?.IsValid == true;

    public override uint Magic => Hash.AkmmioFourcc('E', 'N', 'V', 'S');

    public EnvSettingsConversionTable? ConversionTable { get; private set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var conversionTable = new EnvSettingsConversionTable();

        if (!conversionTable.Read(reader, size))
        {
            return false;
        }

        ConversionTable = conversionTable;

        return true;
    }

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        ConversionTable?.Write(writer);
    }
}
