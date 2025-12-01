using System.Text;

namespace SoundsUnpack.WWise.Chunks;

public class CustomPlatformChunk
{
    public string PlatformName { get; set; }

    public bool Read(BinaryReader reader, uint size)
    {
        var stringSize = reader.ReadUInt32();
        var customPlatformString = Encoding.UTF8.GetString(reader.ReadBytes((int)stringSize));

        PlatformName = customPlatformString;

        return true;
    }
}