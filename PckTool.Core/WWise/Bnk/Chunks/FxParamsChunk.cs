using PckTool.Core.WWise.Common;

namespace PckTool.Core.WWise.Bnk.Chunks;

/// <summary>
///     Represents the FXPR (FX Parameters) chunk of a soundbank.
///     Currently stored as raw bytes since the structure is not fully understood.
/// </summary>
public class FxParamsChunk : BaseChunk
{
    public override bool IsValid => true;

    public override uint Magic => Hash.AkmmioFourcc('F', 'X', 'P', 'R');

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
