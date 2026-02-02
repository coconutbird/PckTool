using PckTool.Core.WWise.Bnk.Enums;
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
    ///     Secondary index by ID only for backward-compatible lookups.
    ///     When multiple types share an ID, stores the first one encountered.
    /// </summary>
    private Dictionary<uint, HircItem>? _itemByIdIndex;

    /// <summary>
    ///     Index of items by (IdType, ID) for O(1) lookup.
    ///     Different ID type categories can share the same numeric ID.
    /// </summary>
    private Dictionary<(IdType IdType, uint Id), HircItem>? _itemIndex;

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
        BuildIndex(items);
    }

    /// <summary>
    ///     Gets a HIRC item by its ID type and ID.
    ///     This is the preferred lookup method as different ID types can share the same numeric ID.
    /// </summary>
    /// <param name="idType">The ID type category.</param>
    /// <param name="id">The item ID.</param>
    /// <returns>The HIRC item, or null if not found.</returns>
    public HircItem? GetItemByIdType(IdType idType, uint id)
    {
        return _itemIndex?.GetValueOrDefault((idType, id));
    }

    /// <summary>
    ///     Gets a HIRC item by its ID.
    ///     Note: Different ID types can share the same numeric ID. If you need a specific type,
    ///     use <see cref="GetItemByIdType" /> instead.
    /// </summary>
    /// <param name="id">The item ID to look up.</param>
    /// <returns>The first HIRC item with this ID, or null if not found.</returns>
    public HircItem? GetItemById(uint id)
    {
        return _itemByIdIndex?.GetValueOrDefault(id);
    }

    private void BuildIndex(List<HircItem> items)
    {
        _itemIndex = new Dictionary<(IdType IdType, uint Id), HircItem>();
        _itemByIdIndex = new Dictionary<uint, HircItem>();

        foreach (var item in items)
        {
            var idType = item.Type.GetIdType();

            // Primary index: (IdType, Id) - unique per ID type category
            _itemIndex[(idType, item.Id)] = item;

            // Secondary index: Id only - first one wins for backward compatibility
            _itemByIdIndex.TryAdd(item.Id, item);
        }
    }

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        var items = new List<HircItem>();

        var numberOfReleasableHircItem = reader.ReadUInt32();
        var hasFeedback = soundBank.HasFeedback;

        for (var i = 0; i < numberOfReleasableHircItem; ++i)
        {
            var item = HircItem.Read(reader, hasFeedback);

            if (item is null)
            {
                return false;
            }

            items.Add(item);
        }

        Items = items;

        // Build index for O(1) lookup by (Type, ID) and by ID only
        BuildIndex(items);

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
