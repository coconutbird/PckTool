using PckTool.Core.WWise.Bnk.Structs;
using PckTool.Core.WWise.Pck;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class MediaIndexChunk : BaseChunk
{
    public override bool IsValid => LoadedMedia is not null;

    public List<MediaHeader>? LoadedMedia { get; private set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var loadedMedia = new List<MediaHeader>();

        var numberOfMedia = size / MediaHeader.SizeOf;

        for (var i = 0; i < numberOfMedia; ++i)
        {
            var mediaHeader = new MediaHeader();

            if (!mediaHeader.Read(reader))
            {
                return false;
            }

            loadedMedia.Add(mediaHeader);
        }

        LoadedMedia = loadedMedia;

        return true;
    }
}
