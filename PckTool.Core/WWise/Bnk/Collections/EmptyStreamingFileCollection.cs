using System.Collections;

using PckTool.Abstractions;

namespace PckTool.Core.WWise.Bnk.Collections;

/// <summary>
///     An empty streaming file collection for standalone BNK files.
/// </summary>
internal class EmptyStreamingFileCollection : IStreamingFileCollection
{
    public static readonly EmptyStreamingFileCollection Instance = new();

    private EmptyStreamingFileCollection() { }

    /// <inheritdoc />
    public int Count => 0;

    /// <inheritdoc />
    public IReadOnlyList<IStreamingFileEntry> Entries => Array.Empty<IStreamingFileEntry>();

    /// <inheritdoc />
    public IStreamingFileEntry? this[uint sourceId] => null;

    /// <inheritdoc />
    public IEnumerable<uint> SourceIds => Enumerable.Empty<uint>();

    /// <inheritdoc />
    public bool Contains(uint sourceId)
    {
        return false;
    }

    /// <inheritdoc />
    public bool TryGet(uint sourceId, out IStreamingFileEntry? entry)
    {
        entry = null;

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<IStreamingFileEntry> GetEnumerator()
    {
        yield break;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
///     An empty external file collection for standalone BNK files.
/// </summary>
internal class EmptyExternalFileCollection : IExternalFileCollection
{
    public static readonly EmptyExternalFileCollection Instance = new();

    private EmptyExternalFileCollection() { }

    /// <inheritdoc />
    public int Count => 0;

    /// <inheritdoc />
    public IReadOnlyList<IExternalFileEntry> Entries => Array.Empty<IExternalFileEntry>();

    /// <inheritdoc />
    public IExternalFileEntry? this[ulong fileId] => null;

    /// <inheritdoc />
    public IEnumerable<ulong> FileIds => Enumerable.Empty<ulong>();

    /// <inheritdoc />
    public bool Contains(ulong fileId)
    {
        return false;
    }

    /// <inheritdoc />
    public bool TryGet(ulong fileId, out IExternalFileEntry? entry)
    {
        entry = null;

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<IExternalFileEntry> GetEnumerator()
    {
        yield break;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
