namespace PckTool.WWise.Structs;

public class MediaInformation
{
    public uint SourceId { get; set; }
    public uint InMemoryMediaSize { get; set; }
    public byte SourceBits { get; set; }

    public bool IsLanguageSpecific
    {
        get => (SourceBits & 0x01) != 0;
        set
        {
            if (value)
            {
                SourceBits |= 0x01;
            }
            else
            {
                SourceBits &= 0xFE;
            }
        }
    }

    public bool Prefetch
    {
        get => (SourceBits & 0x02) != 0;
        set
        {
            if (value)
            {
                SourceBits |= 0x02;
            }
            else
            {
                SourceBits &= 0xFD;
            }
        }
    }

    public bool NonCachable
    {
        get => (SourceBits & 0x08) != 0; // bit 3 for v113+
        set
        {
            if (value)
            {
                SourceBits |= 0x08;
            }
            else
            {
                SourceBits &= 0xF7;
            }
        }
    }

    public bool HasSource
    {
        get => (SourceBits & 0x80) != 0;
        set
        {
            if (value)
            {
                SourceBits |= 0x80;
            }
            else
            {
                SourceBits &= 0x7F;
            }
        }
    }

    public bool Read(BinaryReader reader)
    {
        var sourceId = reader.ReadUInt32();
        var inMemoryMediaSize = reader.ReadUInt32();
        var sourceBits = reader.ReadByte();

        SourceId = sourceId;
        InMemoryMediaSize = inMemoryMediaSize;
        SourceBits = sourceBits;

        return true;
    }
}
