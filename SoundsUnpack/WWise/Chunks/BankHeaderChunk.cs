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

        BankGeneratorVersion = reader.ReadUInt32();
        SoundBankId = reader.ReadUInt32();
        LanguageId = reader.ReadUInt32();
        FeedbackInBank = reader.ReadUInt32();
        ProjectId = reader.ReadUInt32();

        // Padding
        reader.BaseStream.Position += 0x0C;

        return true;
    }

    public const int SizeOf = 0x20;
}