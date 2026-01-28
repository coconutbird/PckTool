using PckTool.Core.WWise.Bnk.Hirc.Params;
using PckTool.Core.WWise.Common;

namespace PckTool.Core.WWise.Bnk.Chunks;

public class MediaIndexChunk : BaseChunk
{
    public override bool IsValid => LoadedMedia is not null;

    public override uint Magic => Hash.AkmmioFourcc('D', 'I', 'D', 'X');

    public List<MediaHeader>? LoadedMedia { get; private set; }

    /// <summary>
    ///     Sets the loaded media headers. Used for serialization when creating banks from scratch.
    /// </summary>
    /// <param name="headers">The media headers to set.</param>
    internal void SetLoadedMedia(List<MediaHeader> headers)
    {
        LoadedMedia = headers;
    }

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

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        if (LoadedMedia is null) return;

        foreach (var header in LoadedMedia)
        {
            header.Write(writer);
        }
    }
}
