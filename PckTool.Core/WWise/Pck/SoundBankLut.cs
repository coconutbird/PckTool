namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Lookup table for sound bank entries (uses 32-bit file IDs).
/// </summary>
public class SoundBankLut : FileLut<uint, SoundBankEntry>
{
    protected override int KeySize => 4;

    protected override uint ReadKey(BinaryReader reader)
    {
        return reader.ReadUInt32();
    }

    protected override void WriteKey(BinaryWriter writer, uint key)
    {
        writer.Write(key);
    }

    protected override SoundBankEntry CreateEntry(uint id, uint blockSize, uint startBlock, uint languageId)
    {
        return new SoundBankEntry { Id = id, BlockSize = blockSize, StartBlock = startBlock, LanguageId = languageId };
    }
}
