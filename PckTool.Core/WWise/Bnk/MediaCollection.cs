using System.Collections;
using System.Collections.Specialized;

using PckTool.Abstractions;

namespace PckTool.Core.WWise.Bnk;

/// <summary>
///     Dictionary-backed collection for embedded media (sourceId -> byte[]).
///     Preserves insertion order for serialization.
/// </summary>
public class MediaCollection : IMediaCollection, INotifyCollectionChanged
{
    private readonly Dictionary<uint, byte[]> _media = new();
    private readonly List<uint> _orderedIds = new(); // Preserve insertion order for serialization

    /// <summary>
    ///     Gets the number of media entries.
    /// </summary>
    public int Count => _media.Count;

    /// <summary>
    ///     Gets all source IDs in this collection.
    /// </summary>
    public IEnumerable<uint> SourceIds => _orderedIds.AsReadOnly();

    /// <summary>
    ///     Gets or sets media data by source ID.
    /// </summary>
    public byte[] this[uint sourceId]
    {
        get =>
            _media.TryGetValue(sourceId, out var data)
                ? data
                : throw new KeyNotFoundException($"Media with source ID 0x{sourceId:X8} not found.");
        set => Set(sourceId, value);
    }

    public IEnumerator<KeyValuePair<uint, byte[]>> GetEnumerator()
    {
        foreach (var id in _orderedIds)
        {
            yield return new KeyValuePair<uint, byte[]>(id, _media[id]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Adds media data with the specified source ID.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if media with the same ID already exists.</exception>
    public void Add(uint sourceId, byte[] data)
    {
        if (_media.ContainsKey(sourceId))
        {
            throw new ArgumentException($"Media with source ID {sourceId:X8} already exists.", nameof(sourceId));
        }

        _media[sourceId] = data;
        _orderedIds.Add(sourceId);

        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                new KeyValuePair<uint, byte[]>(sourceId, data)));
    }

    /// <summary>
    ///     Removes media by source ID.
    /// </summary>
    /// <returns>True if the media was removed, false if it didn't exist.</returns>
    public bool Remove(uint sourceId)
    {
        if (!_media.TryGetValue(sourceId, out var data))
        {
            return false;
        }

        _media.Remove(sourceId);
        _orderedIds.Remove(sourceId);

        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                new KeyValuePair<uint, byte[]>(sourceId, data)));

        return true;
    }

    /// <summary>
    ///     Checks if media with the specified source ID exists.
    /// </summary>
    public bool Contains(uint sourceId)
    {
        return _media.ContainsKey(sourceId);
    }

    /// <summary>
    ///     Tries to get media data by source ID.
    /// </summary>
    public bool TryGet(uint sourceId, out byte[]? data)
    {
        return _media.TryGetValue(sourceId, out data);
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///     Adds or replaces media data.
    /// </summary>
    public void Set(uint sourceId, byte[] data)
    {
        if (_media.ContainsKey(sourceId))
        {
            var oldData = _media[sourceId];
            _media[sourceId] = data;

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<uint, byte[]>(sourceId, data),
                    new KeyValuePair<uint, byte[]>(sourceId, oldData)));
        }
        else
        {
            Add(sourceId, data);
        }
    }

    /// <summary>
    ///     Removes all media from the collection.
    /// </summary>
    public void Clear()
    {
        _media.Clear();
        _orderedIds.Clear();

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>
    ///     Internal method for bulk loading during parsing.
    ///     Does not raise collection changed events.
    /// </summary>
    internal void AddRange(IEnumerable<KeyValuePair<uint, byte[]>> entries)
    {
        foreach (var (id, data) in entries)
        {
            _media[id] = data;
            _orderedIds.Add(id);
        }
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }
}
