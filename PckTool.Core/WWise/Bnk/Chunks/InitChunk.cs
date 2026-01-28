using PckTool.Core.WWise.Util;

namespace PckTool.Core.WWise.Bnk.Chunks;

/// <summary>
///     Represents the INIT (Plugin Initialization) chunk of a soundbank.
///     Contains plugin information for version 118+.
///     Currently stored as raw bytes since we don't need to modify plugin info.
/// </summary>
public class InitChunk : BaseChunk
{
    public override bool IsValid => true;

    public override uint Magic => Hash.AkmmioFourcc('I', 'N', 'I', 'T');

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

