namespace PckTool.Core.WWise.Bnk.Chunks;

/// <summary>
///     A base class for all chunks.
/// </summary>
public abstract class BaseChunk
{
    public abstract bool IsValid { get; }

    /// <summary>
    ///     The chunk magic (4-character code).
    /// </summary>
    public abstract uint Magic { get; }

    /// <summary>
    ///     Reads the chunk from the reader.
    /// </summary>
    /// <param name="soundBank"></param>
    /// <param name="reader"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public bool Read(SoundBank soundBank, BinaryReader reader, uint size)
    {
        if (reader.BaseStream.Position + size > reader.BaseStream.Length)
        {
            Log.Error(
                "Not enough data in stream for {0}. Need {1} bytes, have {2}",
                GetType().Name,
                size,
                reader.BaseStream.Length - reader.BaseStream.Position);

            return false;
        }

        var startPosition = reader.BaseStream.Position;

        if (!ReadInternal(soundBank, reader, size, startPosition))
        {
            return false;
        }

        var expectedPosition = startPosition + size;
        var isInExpectedPosition = reader.BaseStream.Position == expectedPosition;

        if (!isInExpectedPosition)
        {
            var diffTerm = reader.BaseStream.Position > expectedPosition ? "overrun" : "underrun";

            Log.Error(
                "Chunk {0} {1} by {2} bytes. Expected {3}, got {4}.",
                GetType().Name,
                diffTerm,
                Math.Abs(reader.BaseStream.Position - expectedPosition),
                expectedPosition,
                reader.BaseStream.Position);
        }

        return isInExpectedPosition;
    }

    /// <summary>
    ///     Writes the chunk to the writer (including magic and size header).
    /// </summary>
    /// <param name="soundBank">The soundbank context.</param>
    /// <param name="writer">The binary writer.</param>
    public void Write(SoundBank soundBank, BinaryWriter writer)
    {
        // Write magic
        writer.Write(Magic);

        // Reserve space for size (we'll fill it in after writing content)
        var sizePosition = writer.BaseStream.Position;
        writer.Write(0u);

        var contentStart = writer.BaseStream.Position;

        // Write chunk content
        WriteInternal(soundBank, writer);

        // Calculate and write size
        var contentEnd = writer.BaseStream.Position;
        var size = (uint) (contentEnd - contentStart);

        writer.BaseStream.Position = sizePosition;
        writer.Write(size);
        writer.BaseStream.Position = contentEnd;
    }

    /// <summary>
    ///     Implement this method to read the chunk data.
    /// </summary>
    /// <param name="soundBank"></param>
    /// <param name="reader"></param>
    /// <param name="size"></param>
    /// <param name="startPosition"></param>
    /// <returns></returns>
    protected abstract bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition);

    /// <summary>
    ///     Implement this method to write the chunk data (without magic/size header).
    /// </summary>
    /// <param name="soundBank"></param>
    /// <param name="writer"></param>
    protected abstract void WriteInternal(SoundBank soundBank, BinaryWriter writer);
}
