namespace SoundsUnpack.WWise.Chunks;

public class BankHeaderChunk
{
    public uint BankGeneratorVersion { get; set; }
    public uint SoundBankId { get; set; }
    public uint LanguageId { get; set; }
    public uint FeedbackInBank { get; set; }
    public uint ProjectId { get; set; }

    public bool Read(BinaryReader reader, uint size)
    {
        if (reader.BaseStream.Position + SizeOf > reader.BaseStream.Length)
        {
            return false;
        }

        var basePosition = reader.BaseStream.Position;

        BankGeneratorVersion = reader.ReadUInt32();
        SoundBankId = reader.ReadUInt32();
        LanguageId = reader.ReadUInt32();
        FeedbackInBank = reader.ReadUInt32();
        ProjectId = reader.ReadUInt32();

        // Padding
        var paddingSize = size - (reader.BaseStream.Position - basePosition);

        reader.BaseStream.Seek(paddingSize, SeekOrigin.Current);

        return true;
    }

    public const int SizeOf = 0x20;
}