using PckTool.Core.WWise.Pck;

namespace PckTool.Core.WWise.Bnk.Chunks;

/// <summary>
///     A base class for all chunks.
/// </summary>
public abstract class BaseChunk
{
    public abstract bool IsValid { get; }

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
    ///     Implement this method to read the chunk data.
    /// </summary>
    /// <param name="soundBank"></param>
    /// <param name="reader"></param>
    /// <param name="size"></param>
    /// <param name="startPosition"></param>
    /// <returns></returns>
    protected abstract bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition);
}
