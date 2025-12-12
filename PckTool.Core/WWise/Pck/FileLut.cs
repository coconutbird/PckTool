using System.Collections;
using System.Numerics;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Base class for file lookup tables in a package.
///     Provides dictionary-based access to entries by ID.
/// </summary>
/// <typeparam name="TKey">The type of the file ID (uint or ulong).</typeparam>
/// <typeparam name="TEntry">The type of entry stored in this LUT.</typeparam>
public abstract class FileLut<TKey, TEntry> : IEnumerable<TEntry>
    where TKey : struct, INumber<TKey>
    where TEntry : FileEntry<TKey>
{
    private readonly Dictionary<TKey, TEntry> _entries = new();
    private readonly List<TEntry> _orderedEntries = []; // Preserve insertion order for serialization

    /// <summary>
    ///     All entries as a dictionary for O(1) lookup by ID.
    /// </summary>
    public IReadOnlyDictionary<TKey, TEntry> ById => _entries;

    /// <summary>
    ///     All entries in insertion order (for serialization).
    /// </summary>
    public IReadOnlyList<TEntry> Entries => _orderedEntries;

    /// <summary>
    ///     Number of entries in this LUT.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    ///     Gets an entry by ID, or null if not found.
    /// </summary>
    public TEntry? this[TKey id] => _entries.GetValueOrDefault(id);

    /// <summary>
    ///     Returns true if any entries have been modified.
    /// </summary>
    public bool HasModifications => _orderedEntries.Any(e => e.IsModified);

    /// <summary>
    ///     Size of the file ID in bytes (4 for uint, 8 for ulong).
    /// </summary>
    protected abstract int KeySize { get; }

    public IEnumerator<TEntry> GetEnumerator()
    {
        return _orderedEntries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Adds an entry to the LUT.
    /// </summary>
    public void Add(TEntry entry)
    {
        _entries[entry.Id] = entry;
        _orderedEntries.Add(entry);
    }

    /// <summary>
    ///     Removes an entry by ID.
    /// </summary>
    public bool Remove(TKey id)
    {
        if (!_entries.TryGetValue(id, out var entry))
        {
            return false;
        }

        _entries.Remove(id);
        _orderedEntries.Remove(entry);

        return true;
    }

    /// <summary>
    ///     Clears all entries.
    /// </summary>
    public void Clear()
    {
        _entries.Clear();
        _orderedEntries.Clear();
    }

    /// <summary>
    ///     Checks if an entry with the given ID exists.
    /// </summary>
    public bool Contains(TKey id)
    {
        return _entries.ContainsKey(id);
    }

    /// <summary>
    ///     Reads the LUT from a binary reader.
    /// </summary>
    public bool Read(BinaryReader reader, uint size)
    {
        Clear();

        var baseOffset = reader.BaseStream.Position;
        var fileCount = reader.ReadUInt32();

        for (var i = 0; i < fileCount; i++)
        {
            var fileId = ReadKey(reader);
            var blockSize = reader.ReadUInt32();
            var fileSize = reader.ReadInt32();
            var startBlock = reader.ReadUInt32();
            var languageId = reader.ReadUInt32();

            // Read the actual file data
            var position = reader.BaseStream.Position;
            reader.BaseStream.Seek(startBlock, SeekOrigin.Begin);
            var buffer = reader.ReadBytes(fileSize);
            reader.BaseStream.Position = position;

            var entry = CreateEntry(fileId, blockSize, startBlock, languageId);
            entry.SetOriginalData(buffer);

            if (!entry.IsValid)
            {
                return false;
            }

            Add(entry);
        }

        reader.BaseStream.Position = baseOffset + size;

        return true;
    }

    /// <summary>
    ///     Writes the LUT header to a binary writer.
    ///     Returns the number of bytes written.
    /// </summary>
    public uint Write(BinaryWriter writer)
    {
        var startPosition = writer.BaseStream.Position;

        writer.Write((uint) Count);

        foreach (var entry in _orderedEntries)
        {
            WriteKey(writer, entry.Id);
            writer.Write(entry.BlockSize);
            writer.Write((int) entry.Size);
            writer.Write(entry.StartBlock);
            writer.Write(entry.LanguageId);
        }

        return (uint) (writer.BaseStream.Position - startPosition);
    }

    /// <summary>
    ///     Calculates the header size in bytes.
    /// </summary>
    public uint CalculateHeaderSize()
    {
        // 4 bytes for count + per entry: KeySize + BlockSize(4) + FileSize(4) + StartBlock(4) + LanguageId(4)
        var entrySize = KeySize + 16;

        return 4 + (uint) (Count * entrySize);
    }

    /// <summary>
    ///     Reads a key from the binary reader.
    /// </summary>
    protected abstract TKey ReadKey(BinaryReader reader);

    /// <summary>
    ///     Writes a key to the binary writer.
    /// </summary>
    protected abstract void WriteKey(BinaryWriter writer, TKey key);

    /// <summary>
    ///     Creates a new entry instance.
    /// </summary>
    protected abstract TEntry CreateEntry(TKey id, uint blockSize, uint startBlock, uint languageId);
}
