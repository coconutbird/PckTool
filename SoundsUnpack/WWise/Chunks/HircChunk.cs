using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Chunks;

public class HircChunk
{
    public List<LoadedItem> LoadedItems { get; set; } = [];

    public bool Read(BinaryReader reader, uint size)
    {
        LoadedItems.Clear();

        var numberOfReleasableHircItem = reader.ReadUInt32();

        for (var i = 0; i < numberOfReleasableHircItem; ++i)
        {
            var loadedItem = new LoadedItem();

            Console.WriteLine("Idx: " + i);

            if (!loadedItem.Read(reader))
            {
                return false;
            }

            LoadedItems.Add(loadedItem);
        }

        return true;
    }
}