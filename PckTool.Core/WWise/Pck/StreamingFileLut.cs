namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Lookup table for streaming file entries (uses 32-bit file IDs).
/// </summary>
public class StreamingFileLut : FileLut<uint, StreamingFileEntry>
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

    protected override StreamingFileEntry CreateEntry(uint id, uint blockSize, uint startBlock, uint languageId)
    {
        return new StreamingFileEntry
        {
            Id = id, BlockSize = blockSize, StartBlock = startBlock, LanguageId = languageId
        };
    }
}
