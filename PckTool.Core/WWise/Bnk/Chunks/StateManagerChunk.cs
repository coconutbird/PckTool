using PckTool.Core.WWise.Util;

namespace PckTool.Core.WWise.Bnk.Chunks;

/// <summary>
///     Represents the STMG (State Manager / Global Settings) chunk of a soundbank.
///     Contains global state and switch group settings.
///     Currently stored as raw bytes to ensure round-trip correctness.
/// </summary>
public class StateManagerChunk : BaseChunk
{
    public override bool IsValid => true;

    public override uint Magic => Hash.AkmmioFourcc('S', 'T', 'M', 'G');

    /// <summary>
    ///     Raw chunk data.
    /// </summary>
    public byte[]? RawData { get; set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        if (size > 0)
        {
            RawData = reader.ReadBytes((int) size);
        }

        return true;
    }

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        if (RawData is not null)
        {
            writer.Write(RawData);
        }
    }
}
