using System.Text;

namespace SoundsUnpack.WWise.Chunks;

public class CustomPlatformChunk : BaseChunk
{
    public override bool IsValid => !string.IsNullOrEmpty(PlatformName);

    public string PlatformName { get; set; } = string.Empty;

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var stringSize = reader.ReadUInt32();
        var customPlatformString = Encoding.UTF8.GetString(reader.ReadBytes((int) stringSize));

        PlatformName = customPlatformString;

        return true;
    }
}
