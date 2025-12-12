namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Lookup table for external file entries (uses 64-bit file IDs).
/// </summary>
public class ExternalFileLut : FileLut<ulong, ExternalFileEntry>
{
    protected override int KeySize => 8;

    protected override ulong ReadKey(BinaryReader reader)
    {
        return reader.ReadUInt64();
    }

    protected override void WriteKey(BinaryWriter writer, ulong key)
    {
        writer.Write(key);
    }

    protected override ExternalFileEntry CreateEntry(ulong id, uint blockSize, uint startBlock, uint languageId)
    {
        return new ExternalFileEntry
        {
            Id = id, BlockSize = blockSize, StartBlock = startBlock, LanguageId = languageId
        };
    }
}
