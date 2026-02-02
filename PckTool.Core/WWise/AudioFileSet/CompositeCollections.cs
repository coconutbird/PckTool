using System.Collections;

using PckTool.Abstractions;

namespace PckTool.Core.WWise.AudioFileSet;

/// <summary>
///     A composite soundbank collection that aggregates entries from multiple audio files.
/// </summary>
internal class CompositeSoundBankCollection : ISoundBankCollection
{
    private readonly List<IAudioFile> _files;

    public CompositeSoundBankCollection(List<IAudioFile> files)
    {
        _files = files;
    }

    /// <inheritdoc />
    public int Count => _files.Sum(f => f.SoundBanks.Count);

    /// <inheritdoc />
    public IReadOnlyList<ISoundBankEntry> Entries => _files.SelectMany(f => f.SoundBanks.Entries).ToList();

    /// <inheritdoc />
    public ISoundBankEntry? this[uint bankId]
    {
        get
        {
            foreach (var file in _files)
            {
                var entry = file.SoundBanks[bankId];

                if (entry is not null)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<uint> BankIds => _files.SelectMany(f => f.SoundBanks.BankIds).Distinct();

    /// <inheritdoc />
    public bool Contains(uint bankId)
    {
        return _files.Any(f => f.SoundBanks.Contains(bankId));
    }

    /// <inheritdoc />
    public bool TryGet(uint bankId, out ISoundBankEntry? entry)
    {
        foreach (var file in _files)
        {
            if (file.SoundBanks.TryGet(bankId, out entry))
            {
                return true;
            }
        }

        entry = null;

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<ISoundBankEntry> GetEnumerator()
    {
        return _files.SelectMany(f => f.SoundBanks).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
///     A composite streaming file collection that aggregates entries from multiple audio files.
/// </summary>
internal class CompositeStreamingFileCollection : IStreamingFileCollection
{
    private readonly List<IAudioFile> _files;

    public CompositeStreamingFileCollection(List<IAudioFile> files)
    {
        _files = files;
    }

    /// <inheritdoc />
    public int Count => _files.Sum(f => f.StreamingFiles.Count);

    /// <inheritdoc />
    public IReadOnlyList<IStreamingFileEntry> Entries => _files.SelectMany(f => f.StreamingFiles.Entries).ToList();

    /// <inheritdoc />
    public IStreamingFileEntry? this[uint sourceId]
    {
        get
        {
            foreach (var file in _files)
            {
                var entry = file.StreamingFiles[sourceId];

                if (entry is not null)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<uint> SourceIds => _files.SelectMany(f => f.StreamingFiles.SourceIds).Distinct();

    /// <inheritdoc />
    public bool Contains(uint sourceId)
    {
        return _files.Any(f => f.StreamingFiles.Contains(sourceId));
    }

    /// <inheritdoc />
    public bool TryGet(uint sourceId, out IStreamingFileEntry? entry)
    {
        foreach (var file in _files)
        {
            if (file.StreamingFiles.TryGet(sourceId, out entry))
            {
                return true;
            }
        }

        entry = null;

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<IStreamingFileEntry> GetEnumerator()
    {
        return _files.SelectMany(f => f.StreamingFiles).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

/// <summary>
///     A composite external file collection that aggregates entries from multiple audio files.
/// </summary>
internal class CompositeExternalFileCollection : IExternalFileCollection
{
    private readonly List<IAudioFile> _files;

    public CompositeExternalFileCollection(List<IAudioFile> files)
    {
        _files = files;
    }

    /// <inheritdoc />
    public int Count => _files.Sum(f => f.ExternalFiles.Count);

    /// <inheritdoc />
    public IReadOnlyList<IExternalFileEntry> Entries => _files.SelectMany(f => f.ExternalFiles.Entries).ToList();

    /// <inheritdoc />
    public IExternalFileEntry? this[ulong fileId]
    {
        get
        {
            foreach (var file in _files)
            {
                var entry = file.ExternalFiles[fileId];

                if (entry is not null)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<ulong> FileIds => _files.SelectMany(f => f.ExternalFiles.FileIds).Distinct();

    /// <inheritdoc />
    public bool Contains(ulong fileId)
    {
        return _files.Any(f => f.ExternalFiles.Contains(fileId));
    }

    /// <inheritdoc />
    public bool TryGet(ulong fileId, out IExternalFileEntry? entry)
    {
        foreach (var file in _files)
        {
            if (file.ExternalFiles.TryGet(fileId, out entry))
            {
                return true;
            }
        }

        entry = null;

        return false;
    }

    /// <inheritdoc />
    public IEnumerator<IExternalFileEntry> GetEnumerator()
    {
        return _files.SelectMany(f => f.ExternalFiles).GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
