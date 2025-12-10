using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Chunks;

public class HircChunk : BaseChunk
{
    public override bool IsValid => LoadedItems is not null;

    public List<LoadedItem>? LoadedItems { get; private set; }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var loadedItems = new List<LoadedItem>();

        var numberOfReleasableHircItem = reader.ReadUInt32();

        for (var i = 0; i < numberOfReleasableHircItem; ++i)
        {
            var loadedItem = new LoadedItem();

            Console.WriteLine("Idx: " + i);

            if (!loadedItem.Read(reader))
            {
                return false;
            }

            loadedItems.Add(loadedItem);
        }

        LoadedItems = loadedItems;

        return true;
    }
}
