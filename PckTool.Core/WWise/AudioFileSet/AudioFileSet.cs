using PckTool.Abstractions;

namespace PckTool.Core.WWise.AudioFileSet;

/// <summary>
///     A composite audio file that wraps multiple <see cref="IAudioFile" /> instances.
///     Provides a unified view across all contained files for cross-file operations.
/// </summary>
/// <remarks>
///     This is useful for games like Halo Wars 2 that have multiple .pck and .bnk files
///     where events in one file may reference audio in another file.
/// </remarks>
public class AudioFileSet : IAudioFile
{
    private readonly CompositeExternalFileCollection _externalFiles;
    private readonly List<IAudioFile> _files = [];
    private readonly Dictionary<uint, string> _languages = new();
    private readonly CompositeSoundBankCollection _soundBanks;
    private readonly CompositeStreamingFileCollection _streamingFiles;

    /// <summary>
    ///     Creates a new empty audio file set.
    /// </summary>
    public AudioFileSet()
    {
        _soundBanks = new CompositeSoundBankCollection(_files);
        _streamingFiles = new CompositeStreamingFileCollection(_files);
        _externalFiles = new CompositeExternalFileCollection(_files);
    }

    /// <summary>
    ///     Creates a new audio file set with the specified files.
    /// </summary>
    /// <param name="files">The audio files to include in the set.</param>
    public AudioFileSet(IEnumerable<IAudioFile> files) : this()
    {
        foreach (var file in files)
        {
            Add(file);
        }
    }

    /// <summary>
    ///     Gets the audio files contained in this set.
    /// </summary>
    public IReadOnlyList<IAudioFile> Files => _files;

    /// <inheritdoc />
    public string? SourcePath => null;

    /// <inheritdoc />
    public bool HasModifications => _files.Any(f => f.HasModifications);

    /// <inheritdoc />
    public AudioFileType FileType => AudioFileType.Composite;

    /// <inheritdoc />
    public ISoundBankCollection SoundBanks => _soundBanks;

    /// <inheritdoc />
    public IStreamingFileCollection StreamingFiles => _streamingFiles;

    /// <inheritdoc />
    public IExternalFileCollection ExternalFiles => _externalFiles;

    /// <inheritdoc />
    public IReadOnlyDictionary<uint, string> Languages => _languages;

    /// <inheritdoc />
    public int SoundBankCount => _files.Sum(f => f.SoundBankCount);

    /// <inheritdoc />
    public int StreamingFileCount => _files.Sum(f => f.StreamingFileCount);

    /// <inheritdoc />
    public int ExternalFileCount => _files.Sum(f => f.ExternalFileCount);

    /// <inheritdoc />
    public byte[]? FindWem(uint sourceId)
    {
        foreach (var file in _files)
        {
            var data = file.FindWem(sourceId);

            if (data is not null)
            {
                return data;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool ContainsWem(uint sourceId)
    {
        return _files.Any(f => f.ContainsWem(sourceId));
    }

    /// <inheritdoc />
    public WemReplacementResult ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true)
    {
        var totalEmbeddedBanks = 0;
        var replacedInStreaming = false;
        var totalHircUpdates = 0;

        foreach (var file in _files)
        {
            var result = file.ReplaceWem(sourceId, data, updateHircSizes);

            if (result.WasReplaced)
            {
                totalEmbeddedBanks += result.EmbeddedBanksModified;
                replacedInStreaming |= result.ReplacedInStreaming;
                totalHircUpdates += result.HircReferencesUpdated;
            }
        }

        return new WemReplacementResult
        {
            SourceId = sourceId,
            EmbeddedBanksModified = totalEmbeddedBanks,
            ReplacedInStreaming = replacedInStreaming,
            HircReferencesUpdated = totalHircUpdates
        };
    }

    /// <inheritdoc />
    /// <remarks>
    ///     For a composite set, if the path is a directory (or ends with a directory separator),
    ///     saves each contained file to that directory. Otherwise throws <see cref="NotSupportedException" />.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    ///     Thrown when called with a file path. Use <see cref="SaveAll(string, bool)" /> to save to a directory instead.
    /// </exception>
    public void Save(string path)
    {
        // Check if it's a directory path (exists or ends with separator)
        if (Directory.Exists(path) || Path.EndsInDirectorySeparator(path))
        {
            SaveAll(path, false);

            return;
        }

        throw new NotSupportedException(
            "Cannot save a composite AudioFileSet to a single file path. "
            + "Provide a directory path, or use SaveAll() to save each file to its original location.");
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Always thrown for composite sets.</exception>
    public void Save(Stream stream)
    {
        throw new NotSupportedException(
            "Cannot save a composite AudioFileSet to a single stream. "
            + "Use SaveAll() to save each file to its original location.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var file in _files)
        {
            file.Dispose();
        }

        _files.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Adds an audio file to the set.
    /// </summary>
    /// <param name="file">The audio file to add.</param>
    public void Add(IAudioFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        _files.Add(file);

        // Merge languages
        foreach (var (langId, langName) in file.Languages)
        {
            _languages.TryAdd(langId, langName);
        }
    }

    /// <summary>
    ///     Saves all modified files back to their original source paths.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if any modified file does not have a source path.
    /// </exception>
    public void SaveAll()
    {
        foreach (var file in _files)
        {
            if (!file.HasModifications)
            {
                continue;
            }

            if (string.IsNullOrEmpty(file.SourcePath))
            {
                throw new InvalidOperationException(
                    "Cannot save file without a source path. Use SaveAll(directory) instead.");
            }

            file.Save(file.SourcePath);
        }
    }

    /// <summary>
    ///     Saves all files to the specified directory.
    /// </summary>
    /// <param name="directory">The directory to save files to.</param>
    /// <param name="onlyModified">If true, only saves files that have modifications.</param>
    public void SaveAll(string directory, bool onlyModified = true)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        foreach (var file in _files)
        {
            if (onlyModified && !file.HasModifications)
            {
                continue;
            }

            var fileName = file.SourcePath is not null
                ? Path.GetFileName(file.SourcePath)
                : $"file_{_files.IndexOf(file)}.bin";

            var outputPath = Path.Combine(directory, fileName);
            file.Save(outputPath);
        }
    }
}
