using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Chunks;

/// <summary>
///     Represents the HIRC (Hierarchy) chunk of a soundbank.
///     Contains the hierarchy of sound objects (events, actions, sounds, containers, etc.).
///     This is a data-only class that parses and stores HIRC data.
/// </summary>
public class HircChunk : BaseChunk
{
    /// <summary>
    ///     Index of items by ID for O(1) lookup.
    /// </summary>
    private Dictionary<uint, LoadedItem>? _itemIndex;

    public override bool IsValid => LoadedItems is not null;

    /// <summary>
    ///     All loaded HIRC items in this chunk.
    /// </summary>
    public List<LoadedItem>? LoadedItems { get; private set; }

    /// <summary>
    ///     Gets a loaded item by its ID.
    ///     Returns null if the item is not found.
    /// </summary>
    /// <param name="id">The item ID to look up.</param>
    /// <returns>The loaded item, or null if not found.</returns>
    public LoadedItem? GetItemById(uint id)
    {
        if (_itemIndex is null) return null;

        return _itemIndex.GetValueOrDefault(id);
    }

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

        // Build index for O(1) lookup by ID
        _itemIndex = loadedItems.ToDictionary(item => item.Id);

        return true;
    }
}
