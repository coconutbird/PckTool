using System.Collections;
using System.Collections.Specialized;

using PckTool.Core.WWise.Bnk.Structs;

namespace PckTool.Core.WWise.Bnk;

/// <summary>
///     Observable dictionary-backed collection for HIRC items.
///     Provides O(1) lookup by ID and typed accessors.
/// </summary>
public class HircCollection : IEnumerable<HircItem>, INotifyCollectionChanged
{
    private readonly Dictionary<uint, HircItem> _items = new();
    private readonly List<HircItem> _orderedItems = new(); // Preserve insertion order for serialization

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Gets the number of items in the collection.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    ///     Gets an item by its ID.
    /// </summary>
    public HircItem? this[uint id] => _items.GetValueOrDefault(id);

    /// <summary>
    ///     Gets an item by ID and casts to the specified type.
    /// </summary>
    public T? Get<T>(uint id) where T : HircItem
    {
        return _items.GetValueOrDefault(id) as T;
    }

    /// <summary>
    ///     Adds an item to the collection.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if an item with the same ID already exists.</exception>
    public void Add(HircItem item)
    {
        if (_items.ContainsKey(item.Id))
        {
            throw new ArgumentException($"An item with ID {item.Id:X8} already exists.", nameof(item));
        }

        _items[item.Id] = item;
        _orderedItems.Add(item);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add, item));
    }

    /// <summary>
    ///     Adds or replaces an item in the collection.
    /// </summary>
    public void Set(HircItem item)
    {
        if (_items.TryGetValue(item.Id, out var existing))
        {
            var index = _orderedItems.IndexOf(existing);
            _orderedItems[index] = item;
            _items[item.Id] = item;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Replace, item, existing));
        }
        else
        {
            Add(item);
        }
    }

    /// <summary>
    ///     Removes an item by its ID.
    /// </summary>
    /// <returns>True if the item was removed, false if it didn't exist.</returns>
    public bool Remove(uint id)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            return false;
        }

        _items.Remove(id);
        _orderedItems.Remove(item);

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove, item));

        return true;
    }

    /// <summary>
    ///     Removes all items from the collection.
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        _orderedItems.Clear();

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Checks if an item with the specified ID exists.
    /// </summary>
    public bool Contains(uint id) => _items.ContainsKey(id);

    public IEnumerator<HircItem> GetEnumerator() => _orderedItems.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    /// <summary>
    ///     Internal method for bulk loading during parsing.
    ///     Does not raise collection changed events.
    /// </summary>
    internal void AddRange(IEnumerable<HircItem> items)
    {
        foreach (var item in items)
        {
            _items[item.Id] = item;
            _orderedItems.Add(item);
        }
    }
}

