namespace PckTool.WWise.Chunks;

public class BankHeaderChunk : BaseChunk
{
    private const uint ValidVersion = 0x71;

    public override bool IsValid =>
        BankGeneratorVersion == ValidVersion
        && SoundBankId is not null
        && LanguageId is not null
        && FeedbackInBank is not null
        && ProjectId is not null;

    public uint? BankGeneratorVersion { get; set; }
    public uint? SoundBankId { get; set; }
    public uint? LanguageId { get; set; }
    public uint? FeedbackInBank { get; set; }
    public uint? ProjectId { get; set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        BankGeneratorVersion = reader.ReadUInt32();
        SoundBankId = reader.ReadUInt32();
        LanguageId = reader.ReadUInt32();
        FeedbackInBank = reader.ReadUInt32();
        ProjectId = reader.ReadUInt32();

        // Padding
        var paddingSize = size - (reader.BaseStream.Position - startPosition);

        reader.BaseStream.Seek(paddingSize, SeekOrigin.Current);

        return IsValid;
    }
}
