using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Chunks;

public class MediaIndexChunk
{
    public List<MediaHeader> LoadedMedia { get; } = new();

    public bool Read(BinaryReader reader, uint size)
    {
        var numberOfMedia = size / MediaHeader.SizeOf;
        for (var i = 0; i < numberOfMedia; ++i)
        {
            var mediaHeader = new MediaHeader();
            if (!mediaHeader.Read(reader))
            {
                return false;
            }

            LoadedMedia.Add(mediaHeader);
        }

        return true;
    }
}