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
    private Dictionary<uint, HircItem>? _itemIndex;

    public override bool IsValid => Items is not null;

    /// <summary>
    ///     All HIRC items in this chunk.
    /// </summary>
    public List<HircItem>? Items { get; private set; }

    /// <summary>
    ///     Gets a HIRC item by its ID.
    ///     Returns null if the item is not found.
    /// </summary>
    /// <param name="id">The item ID to look up.</param>
    /// <returns>The HIRC item, or null if not found.</returns>
    public HircItem? GetItemById(uint id)
    {
        if (_itemIndex is null) return null;

        return _itemIndex.GetValueOrDefault(id);
    }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var items = new List<HircItem>();

        var numberOfReleasableHircItem = reader.ReadUInt32();

        for (var i = 0; i < numberOfReleasableHircItem; ++i)
        {
            Console.WriteLine("Idx: " + i);

            var item = HircItem.Read(reader);

            if (item is null)
            {
                return false;
            }

            items.Add(item);
        }

        Items = items;

        // Build index for O(1) lookup by ID
        _itemIndex = items.ToDictionary(item => item.Id);

        return true;
    }
}
