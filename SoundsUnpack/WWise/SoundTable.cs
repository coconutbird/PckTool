using System.Xml.Linq;

namespace SoundsUnpack.WWise;

/// <summary>
///     Maps cue names (event names) to their associated sound file IDs.
///     Provides efficient lookup in both directions:
///     - CueIndex (event hash) -> CueName
///     - FileId (wem source ID) -> CueName
/// </summary>
public class SoundTable
{
    /// <summary>
    ///     Maps CueIndex (FNV1A-32 hash of cue name) to the cue entry.
    /// </summary>
    private readonly Dictionary<uint, CueEntry> _cueIndexMap = new();

    /// <summary>
    ///     Maps FileId (wem source ID) to the cue name for O(1) lookup.
    /// </summary>
    private readonly Dictionary<uint, string> _fileIdToCueName = new();

    /// <summary>
    ///     All registered cue entries.
    /// </summary>
    public IReadOnlyCollection<CueEntry> Cues => _cueIndexMap.Values;

    /// <summary>
    ///     Loads cue name definitions from an XML file.
    ///     Expected format: Sound elements with CueName and CueIndex children.
    /// </summary>
    public bool Load(string path)
    {
        if (!File.Exists(path)) return false;

        var doc = XDocument.Load(path);

        foreach (var sound in doc.Descendants("Sound"))
        {
            var cueName = sound.Element("CueName")?.Value;

            if (string.IsNullOrEmpty(cueName)) continue;

            var cueIndex = Hash.GetIdFromString(cueName);

            if (_cueIndexMap.ContainsKey(cueIndex)) continue;

            _cueIndexMap[cueIndex] = new CueEntry(cueName, cueIndex);
        }

        return true;
    }

    /// <summary>
    ///     Resolves and registers file IDs for a soundbank's events.
    ///     This populates the FileId -> CueName mapping for efficient lookup.
    /// </summary>
    public void ResolveFileIds(SoundBank soundbank)
    {
        if (soundbank.HircChunk?.LoadedItems is null) return;

        foreach (var item in soundbank.HircChunk.LoadedItems)
        {
            if (item.Type != Enums.HircType.Event) continue;

            if (!_cueIndexMap.TryGetValue(item.Id, out var cueEntry)) continue;

            var fileIds = soundbank.HircChunk.ResolveSoundFileIds(soundbank, item);

            foreach (var fileId in fileIds)
            {
                cueEntry.AddFileId(fileId);
                _fileIdToCueName.TryAdd(fileId, cueEntry.CueName);
            }
        }
    }

    /// <summary>
    ///     Gets the cue name associated with a file ID (wem source ID).
    ///     Returns null if the file ID is not associated with any known cue.
    /// </summary>
    public string? GetCueNameByFileId(uint fileId)
    {
        return _fileIdToCueName.GetValueOrDefault(fileId);
    }

    /// <summary>
    ///     Gets the cue entry by its index (FNV1A-32 hash of the cue name).
    /// </summary>
    public CueEntry? GetCueByIndex(uint cueIndex)
    {
        return _cueIndexMap.GetValueOrDefault(cueIndex);
    }

    /// <summary>
    ///     Gets the cue entry by its name.
    /// </summary>
    public CueEntry? GetCueByName(string name)
    {
        return _cueIndexMap.GetValueOrDefault(Hash.GetIdFromString(name));
    }

    /// <summary>
    ///     Represents a cue (event) entry with its name and associated file IDs.
    /// </summary>
    public class CueEntry
    {
        private readonly HashSet<uint> _fileIds = [];

        public CueEntry(string cueName, uint cueIndex)
        {
            CueName = cueName;
            CueIndex = cueIndex;
        }

        /// <summary>
        ///     The human-readable cue name (event name).
        /// </summary>
        public string CueName { get; }

        /// <summary>
        ///     The FNV1A-32 hash of the cue name.
        /// </summary>
        public uint CueIndex { get; }

        /// <summary>
        ///     The file IDs (wem source IDs) associated with this cue.
        /// </summary>
        public IReadOnlySet<uint> FileIds => _fileIds;

        internal void AddFileId(uint fileId)
        {
            _fileIds.Add(fileId);
        }
    }
}