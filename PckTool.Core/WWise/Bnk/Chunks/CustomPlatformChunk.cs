using System.Text;

using PckTool.Core.WWise.Common;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class CustomPlatformChunk : BaseChunk
{
    public override bool IsValid => !string.IsNullOrEmpty(PlatformName);

    public override uint Magic => Hash.AkmmioFourcc('P', 'L', 'A', 'T');

    public string PlatformName { get; set; } = string.Empty;

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var stringSize = reader.ReadUInt32();
        var customPlatformString = Encoding.UTF8.GetString(reader.ReadBytes((int) stringSize));

        PlatformName = customPlatformString;

        return true;
    }

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        var bytes = Encoding.UTF8.GetBytes(PlatformName);
        writer.Write((uint) bytes.Length);
        writer.Write(bytes);
    }
}
