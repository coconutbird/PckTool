using System.Collections;
using System.Collections.Specialized;

using PckTool.Core.WWise.Bnk.Enums;
using PckTool.Core.WWise.Bnk.Hirc.Items;

namespace PckTool.Core.WWise.Bnk.Collections;

/// <summary>
///     Observable dictionary-backed collection for HIRC items.
///     Provides O(1) lookup by (IdType, ID) and backward-compatible ID-only lookups.
///     Note: Different ID type categories can share the same numeric ID (e.g., FxShareSet and AuxBus).
/// </summary>
public class HircCollection : IEnumerable<HircItem>, INotifyCollectionChanged
{
    /// <summary>
    ///     Primary index by (IdType, ID) - unique per ID type category.
    /// </summary>
    private readonly Dictionary<(IdType IdType, uint Id), HircItem> _items = new();

    /// <summary>
    ///     Secondary index by ID only for backward-compatible lookups.
    ///     When multiple types share an ID, stores the first one added.
    /// </summary>
    private readonly Dictionary<uint, HircItem> _itemsById = new();

    private readonly List<HircItem> _orderedItems = new(); // Preserve insertion order for serialization

    /// <summary>
    ///     Gets the number of items in the collection.
    /// </summary>
    public int Count => _orderedItems.Count;

    /// <summary>
    ///     Gets an item by its ID type and ID.
    ///     This is the preferred lookup method as different ID types can share the same numeric ID.
    /// </summary>
    public HircItem? this[IdType idType, uint id] => _items.GetValueOrDefault((idType, id));

    public IEnumerator<HircItem> GetEnumerator()
    {
        return _orderedItems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Gets an item by ID type and ID and casts to the specified type.
    ///     This is the preferred lookup method.
    /// </summary>
    public T? Get<T>(IdType idType, uint id) where T : HircItem
    {
        return _items.GetValueOrDefault((idType, id)) as T;
    }

    /// <summary>
    ///     Adds an item to the collection.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if an item with the same ID type and ID already exists.</exception>
    public void Add(HircItem item)
    {
        var idType = item.Type.GetIdType();
        var key = (idType, item.Id);

        if (_items.ContainsKey(key))
        {
            throw new ArgumentException(
                $"An item with IdType {idType} and ID {item.Id:X8} already exists.",
                nameof(item));
        }

        _items[key] = item;
        _itemsById.TryAdd(item.Id, item); // First one wins for backward compatibility
        _orderedItems.Add(item);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    /// <summary>
    ///     Adds or replaces an item in the collection.
    /// </summary>
    public void Set(HircItem item)
    {
        var idType = item.Type.GetIdType();
        var key = (idType, item.Id);

        if (_items.TryGetValue(key, out var existing))
        {
            var index = _orderedItems.IndexOf(existing);
            _orderedItems[index] = item;
            _items[key] = item;

            // Update secondary index if this was the item stored there
            if (_itemsById.TryGetValue(item.Id, out var byIdItem) && ReferenceEquals(byIdItem, existing))
            {
                _itemsById[item.Id] = item;
            }

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, existing));
        }
        else
        {
            Add(item);
        }
    }

    /// <summary>
    ///     Removes an item by its ID type and ID.
    ///     This is the preferred removal method.
    /// </summary>
    /// <returns>True if the item was removed, false if it didn't exist.</returns>
    public bool Remove(IdType idType, uint id)
    {
        var key = (idType, id);

        if (!_items.TryGetValue(key, out var item))
        {
            return false;
        }

        _items.Remove(key);
        _orderedItems.Remove(item);

        // Update secondary index: remove if this was the stored item, or find another with same ID
        if (_itemsById.TryGetValue(id, out var byIdItem) && ReferenceEquals(byIdItem, item))
        {
            _itemsById.Remove(id);

            // Find another item with the same ID to maintain backward compatibility
            var replacement = _orderedItems.FirstOrDefault(i => i.Id == id);

            if (replacement is not null)
            {
                _itemsById[id] = replacement;
            }
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));

        return true;
    }

    /// <summary>
    ///     Removes an item by its ID.
    ///     Note: If multiple ID types share this ID, only the first one (by insertion order) is removed.
    /// </summary>
    /// <returns>True if the item was removed, false if it didn't exist.</returns>
    public bool Remove(uint id)
    {
        if (!_itemsById.TryGetValue(id, out var item))
        {
            return false;
        }

        return Remove(item.Type.GetIdType(), id);
    }

    /// <summary>
    ///     Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _itemsById.Clear();
        _orderedItems.Clear();

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Checks if an item with the specified ID type and ID exists.
    /// </summary>
    public bool Contains(IdType idType, uint id)
    {
        return _items.ContainsKey((idType, id));
    }

    /// <summary>
    ///     Checks if an item with the specified ID exists (any ID type).
    /// </summary>
    public bool Contains(uint id)
    {
        return _itemsById.ContainsKey(id);
    }

    /// <summary>
    ///     Internal method for bulk loading during parsing.
    ///     Does not raise collection changed events.
    /// </summary>
    internal void AddRange(IEnumerable<HircItem> items)
    {
        foreach (var item in items)
        {
            var idType = item.Type.GetIdType();
            var key = (idType, item.Id);
            _items[key] = item;
            _itemsById.TryAdd(item.Id, item); // First one wins for backward compatibility
            _orderedItems.Add(item);
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
}
