using PckTool.Core.WWise.Bnk.Hirc.Items;
using PckTool.Core.WWise.Common;

namespace PckTool.Core.WWise.Bnk.Chunks;

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

    public override uint Magic => Hash.AkmmioFourcc('H', 'I', 'R', 'C');

    /// <summary>
    ///     All HIRC items in this chunk.
    /// </summary>
    public List<HircItem>? Items { get; private set; }

    /// <summary>
    ///     Sets the items in this chunk. Used for serialization when creating banks from scratch.
    /// </summary>
    /// <param name="items">The items to set.</param>
    internal void SetItems(List<HircItem> items)
    {
        Items = items;
        _itemIndex = items.ToDictionary(item => item.Id);
    }

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

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        if (Items is null) return;

        // Write item count
        writer.Write((uint) Items.Count);

        // Write each item (will throw NotImplementedException for unimplemented types)
        foreach (var item in Items)
        {
            item.Write(writer);
        }
    }
}
