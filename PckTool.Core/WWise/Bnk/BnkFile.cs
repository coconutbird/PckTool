using PckTool.Abstractions;
using PckTool.Core.WWise.Bnk.Collections;
using PckTool.Core.WWise.Bnk.Entries;

namespace PckTool.Core.WWise.Bnk;

/// <summary>
///     Wraps a standalone BNK soundbank file to implement the unified <see cref="IAudioFile" /> interface.
/// </summary>
/// <remarks>
///     This allows standalone .bnk files to be used interchangeably with .pck files
///     in commands and batch operations.
/// </remarks>
public class BnkFile : IAudioFile
{
    private readonly BnkFileSoundBankEntry _entry;
    private readonly BnkFileSoundBankCollection _soundBankCollection;

    /// <summary>
    ///     Creates a new standalone BNK file wrapper.
    /// </summary>
    /// <param name="soundBank">The parsed soundbank.</param>
    /// <param name="sourcePath">The source file path, if loaded from disk.</param>
    public BnkFile(SoundBank soundBank, string? sourcePath = null)
    {
        SoundBank = soundBank ?? throw new ArgumentNullException(nameof(soundBank));
        SourcePath = sourcePath;
        _entry = new BnkFileSoundBankEntry(soundBank, this);
        _soundBankCollection = new BnkFileSoundBankCollection(_entry);
    }

    /// <summary>
    ///     Gets the underlying soundbank.
    /// </summary>
    public SoundBank SoundBank { get; }

    /// <inheritdoc />
    public string? SourcePath { get; }

    /// <inheritdoc />
    public bool HasModifications { get; private set; }

    /// <inheritdoc />
    public AudioFileType FileType => AudioFileType.Bnk;

    /// <inheritdoc />
    public ISoundBankCollection SoundBanks => _soundBankCollection;

    /// <inheritdoc />
    public IStreamingFileCollection StreamingFiles => EmptyStreamingFileCollection.Instance;

    /// <inheritdoc />
    public IExternalFileCollection ExternalFiles => EmptyExternalFileCollection.Instance;

    /// <inheritdoc />
    public IReadOnlyDictionary<uint, string> Languages { get; } = new Dictionary<uint, string> { { 0, "SFX" } };

    /// <inheritdoc />
    public int SoundBankCount => 1;

    /// <inheritdoc />
    public int StreamingFileCount => 0;

    /// <inheritdoc />
    public int ExternalFileCount => 0;

    /// <inheritdoc />
    public byte[]? FindWem(uint sourceId)
    {
        return SoundBank.GetMedia(sourceId);
    }

    /// <inheritdoc />
    public bool ContainsWem(uint sourceId)
    {
        return SoundBank.ContainsMedia(sourceId);
    }

    /// <inheritdoc />
    public WemReplacementResult ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true)
    {
        if (!SoundBank.ContainsMedia(sourceId))
        {
            return new WemReplacementResult { SourceId = sourceId };
        }

        var hircUpdates = SoundBank.ReplaceWem(sourceId, data, updateHircSizes);
        HasModifications = true;

        return new WemReplacementResult
        {
            SourceId = sourceId, EmbeddedBanksModified = 1, HircReferencesUpdated = hircUpdates
        };
    }

    /// <inheritdoc />
    public void Save(string path)
    {
        SoundBank.Save(path);
    }

    /// <inheritdoc />
    public void Save(Stream stream)
    {
        SoundBank.Write(stream);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose for standalone BNK files
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Loads a standalone BNK file from disk.
    /// </summary>
    /// <param name="path">The path to the BNK file.</param>
    /// <returns>A standalone BNK file wrapper.</returns>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidDataException">The file could not be parsed as a BNK.</exception>
    public static BnkFile Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("BNK file not found.", path);
        }

        var soundBank = SoundBank.Load(path);

        if (soundBank is null)
        {
            throw new InvalidDataException($"Failed to parse BNK file: {path}");
        }

        return new BnkFile(soundBank, path);
    }

    internal void MarkModified()
    {
        HasModifications = true;
    }
}
