using PckTool.Core.WWise.Util;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class BankHeaderChunk : BaseChunk
{
    private const uint ValidVersion = 0x71;
    private const int StandardSize = 20; // 5 x uint32

    /// <summary>
    ///     Extra padding bytes read from the original chunk (if any).
    /// </summary>
    private byte[]? _padding;

    public override bool IsValid =>
        BankGeneratorVersion == ValidVersion
        && SoundBankId is not null
        && LanguageId is not null
        && FeedbackInBank is not null
        && ProjectId is not null;

    public override uint Magic => Hash.AkmmioFourcc('B', 'K', 'H', 'D');

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
        var paddingSize = (int) (size - (reader.BaseStream.Position - startPosition));

        if (paddingSize > 0)
        {
            _padding = reader.ReadBytes(paddingSize);
        }

        return IsValid;
    }

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        writer.Write(BankGeneratorVersion ?? ValidVersion);
        writer.Write(SoundBankId ?? 0u);
        writer.Write(LanguageId ?? 0u);
        writer.Write(FeedbackInBank ?? 0u);
        writer.Write(ProjectId ?? 0u);

        // Write padding if we had any
        if (_padding is not null)
        {
            writer.Write(_padding);
        }
    }
}
