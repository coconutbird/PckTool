using PckTool.Core.Games.HaloWars;
using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Bnk.Enums;
using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Services;

/// <summary>
///     Provides a unified API for browsing PCK files, banks, and sounds.
///     Can be used by both CLI and UI applications.
/// </summary>
public class PackageBrowser : IDisposable
{
    private bool _fileIdsResolved;
    private PckFile? _package;
    private Dictionary<uint, SoundBank>? _parsedBanks;
    private SoundTable? _soundTable;

    /// <summary>
    ///     The currently loaded package file path.
    /// </summary>
    public string? PackagePath => _package?.SourcePath;

    /// <summary>
    ///     Whether a package is currently loaded.
    /// </summary>
    public bool IsPackageLoaded => _package is not null;

    /// <summary>
    ///     Whether a sound table is loaded.
    /// </summary>
    public bool IsSoundTableLoaded => _soundTable is not null;

    /// <summary>
    ///     All available languages in the loaded package.
    /// </summary>
    public IReadOnlyDictionary<uint, string> Languages => _package?.Languages ?? new Dictionary<uint, string>();

    public void Dispose()
    {
        _package?.Dispose();
        _package = null;
        _parsedBanks = null;
        _soundTable = null;
    }

    /// <summary>
    ///     Loads a PCK package file.
    /// </summary>
    /// <param name="path">Path to the .pck file.</param>
    /// <returns>True if loaded successfully.</returns>
    public bool LoadPackage(string path)
    {
        _package?.Dispose();
        _package = null;
        _parsedBanks = null;
        _fileIdsResolved = false;

        _package = PckFile.Load(path);

        if (_package is null)
        {
            return false;
        }

        // Pre-parse all banks for efficient access
        _parsedBanks = new Dictionary<uint, SoundBank>();

        foreach (var entry in _package.SoundBanks.Entries)
        {
            var parsed = entry.Parse();

            if (parsed is not null)
            {
                _parsedBanks[entry.Id] = parsed;
            }
        }

        // If sound table is already loaded, resolve file IDs
        if (_soundTable is not null)
        {
            ResolveFileIds();
        }

        return true;
    }

    /// <summary>
    ///     Loads a sound table XML file for cue name resolution.
    /// </summary>
    /// <param name="path">Path to the soundtable.xml file.</param>
    /// <returns>True if loaded successfully.</returns>
    public bool LoadSoundTable(string path)
    {
        _soundTable = new SoundTable();
        _fileIdsResolved = false;

        if (!_soundTable.Load(path))
        {
            _soundTable = null;

            return false;
        }

        // If package is already loaded, resolve file IDs
        if (_package is not null && _parsedBanks is not null)
        {
            ResolveFileIds();
        }

        return true;
    }

    /// <summary>
    ///     Gets all banks in the package.
    /// </summary>
    /// <param name="languageId">Optional language filter.</param>
    /// <returns>Enumerable of bank information.</returns>
    public IEnumerable<BankInfo> GetBanks(uint? languageId = null)
    {
        if (_package is null)
        {
            yield break;
        }

        foreach (var entry in _package.SoundBanks.Entries)
        {
            if (languageId.HasValue && entry.LanguageId != languageId.Value)
            {
                continue;
            }

            var parsed = _parsedBanks?.GetValueOrDefault(entry.Id);
            var soundCount = parsed?.Sounds.Count() ?? 0;

            yield return new BankInfo
            {
                Id = entry.Id,
                LanguageId = entry.LanguageId,
                Language = entry.Language
                           ?? _package.Languages.GetValueOrDefault(
                               entry.LanguageId,
                               $"Unknown ({entry.LanguageId})"),
                Size = entry.Size,
                SoundCount = soundCount,
                IsValid = parsed?.IsValid ?? false
            };
        }
    }

    /// <summary>
    ///     Gets all sounds in a specific bank.
    /// </summary>
    /// <param name="bankId">The bank ID.</param>
    /// <returns>Enumerable of sound information.</returns>
    public IEnumerable<SoundInfo> GetSounds(uint bankId)
    {
        if (_parsedBanks is null || !_parsedBanks.TryGetValue(bankId, out var bank))
        {
            yield break;
        }

        foreach (var sound in bank.Sounds)
        {
            var sourceId = sound.Values.BankSourceData.MediaInformation.SourceId;
            var cueName = _soundTable?.GetCueNameByFileId(sourceId);

            yield return new SoundInfo
            {
                Id = sound.Id,
                SourceId = sourceId,
                Name = cueName,
                StreamType = sound.Values.BankSourceData.StreamType,
                PluginType = sound.Values.BankSourceData.PluginType,
                HasEmbeddedMedia = bank.Media.Contains(sourceId)
            };
        }
    }

    /// <summary>
    ///     Gets the raw media data for a sound.
    /// </summary>
    /// <param name="bankId">The bank ID containing the sound.</param>
    /// <param name="sourceId">The source ID of the sound.</param>
    /// <returns>The raw WEM data, or null if not found.</returns>
    public byte[]? GetSoundData(uint bankId, uint sourceId)
    {
        if (_parsedBanks is null || !_parsedBanks.TryGetValue(bankId, out var bank))
        {
            return null;
        }

        return bank.GetMedia(sourceId);
    }

    /// <summary>
    ///     Gets detailed information about a specific bank.
    /// </summary>
    /// <param name="bankId">The bank ID.</param>
    /// <returns>Bank details, or null if not found.</returns>
    public BankDetails? GetBankDetails(uint bankId)
    {
        if (_package is null)
        {
            return null;
        }

        var entry = _package.SoundBanks[bankId];

        if (entry is null)
        {
            return null;
        }

        var parsed = _parsedBanks?.GetValueOrDefault(bankId);

        return new BankDetails
        {
            Id = entry.Id,
            LanguageId = entry.LanguageId,
            Language = entry.Language
                       ?? _package.Languages.GetValueOrDefault(entry.LanguageId, $"Unknown ({entry.LanguageId})"),
            Size = entry.Size,
            Version = parsed?.Version ?? 0,
            ProjectId = parsed?.ProjectId ?? 0,
            IsValid = parsed?.IsValid ?? false,
            SoundCount = parsed?.Sounds.Count() ?? 0,
            EventCount = parsed?.Events.Count() ?? 0,
            ActionCount = parsed?.Actions.Count() ?? 0,
            MediaCount = parsed?.Media.Count ?? 0,
            Sounds = GetSounds(bankId).ToList()
        };
    }

    /// <summary>
    ///     Gets the cue name for a file ID.
    /// </summary>
    public string? GetCueName(uint fileId)
    {
        return _soundTable?.GetCueNameByFileId(fileId);
    }

    private void ResolveFileIds()
    {
        if (_soundTable is null || _parsedBanks is null || _fileIdsResolved)
        {
            return;
        }

        foreach (var bank in _parsedBanks.Values)
        {
            _soundTable.ResolveFileIds(bank, id => _parsedBanks.GetValueOrDefault(id));
        }

        _fileIdsResolved = true;
    }
}

/// <summary>
///     Basic information about a sound bank.
/// </summary>
public class BankInfo
{
    public required uint Id { get; init; }
    public required uint LanguageId { get; init; }
    public required string Language { get; init; }
    public required long Size { get; init; }
    public required int SoundCount { get; init; }
    public required bool IsValid { get; init; }

    public string IdHex => $"{Id:X8}";

    public string SizeFormatted =>
        Size switch
        {
            < 1024 => $"{Size} B",
            < 1024 * 1024 => $"{Size / 1024.0:F1} KB",
            _ => $"{Size / (1024.0 * 1024.0):F1} MB"
        };
}

/// <summary>
///     Detailed information about a sound bank.
/// </summary>
public class BankDetails : BankInfo
{
    public required uint Version { get; init; }
    public required uint ProjectId { get; init; }
    public required int EventCount { get; init; }
    public required int ActionCount { get; init; }
    public required int MediaCount { get; init; }
    public required IReadOnlyList<SoundInfo> Sounds { get; init; }
}

/// <summary>
///     Information about a sound within a bank.
/// </summary>
public class SoundInfo
{
    public required uint Id { get; init; }
    public required uint SourceId { get; init; }
    public string? Name { get; init; }
    public required StreamType StreamType { get; init; }
    public required PluginType PluginType { get; init; }
    public required bool HasEmbeddedMedia { get; init; }

    public string IdHex => $"{Id:X8}";
    public string SourceIdHex => $"{SourceId:X8}";
    public string DisplayName => Name ?? SourceIdHex;
}
