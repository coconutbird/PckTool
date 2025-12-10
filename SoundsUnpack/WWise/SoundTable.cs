using System.Xml.Linq;

using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise;

/// <summary>
///     Maps cue names (event names) to their associated sound file IDs.
///     Provides efficient lookup in both directions:
///     - CueIndex (event hash) -> CueName
///     - FileId (wem source ID) -> CueName
///     Also handles resolution of Event → Action → Sound → FileId chains
///     with support for cross-bank and cross-language references.
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
            var cueIndex = sound.Element("CueIndex")?.Value;

            if (string.IsNullOrEmpty(cueName)) continue;

            var computedCueIndex = Hash.GetIdFromString(cueName);

            if (computedCueIndex != uint.Parse(cueIndex!))
            {
                throw new Exception("CueIndex does not match computed value");
            }

            if (_cueIndexMap.ContainsKey(computedCueIndex)) continue;

            _cueIndexMap[computedCueIndex] = new CueEntry(cueName, computedCueIndex);
        }

        return true;
    }

    /// <summary>
    ///     Resolves and registers file IDs for a soundbank's events.
    ///     This populates the FileId -> CueName mapping for efficient lookup.
    ///     Uses the provided bank lookup for cross-bank reference resolution.
    /// </summary>
    /// <param name="soundbank">The soundbank to process.</param>
    /// <param name="bankLookup">Function to lookup soundbanks by ID for cross-bank references.</param>
    public void ResolveFileIds(SoundBank soundbank, Func<uint, SoundBank?> bankLookup)
    {
        if (soundbank.HircChunk?.Items is null) return;

        foreach (var item in soundbank.HircChunk.Items)
        {
            // Only process EventItems - use pattern matching for type safety
            if (item is not EventItem eventItem) continue;

            if (!_cueIndexMap.TryGetValue(eventItem.Id, out var cueEntry)) continue;

            // Resolve the event's file IDs using our internal resolution logic
            var fileIds = ResolveSoundFileIds(bankLookup, soundbank, eventItem);

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
    public class CueEntry(string cueName, uint cueIndex)
    {
        private readonly HashSet<uint> _fileIds = [];

        /// <summary>
        ///     The human-readable cue name (event name).
        /// </summary>
        public string CueName { get; } = cueName;

        /// <summary>
        ///     The FNV1A-32 hash of the cue name.
        /// </summary>
        public uint CueIndex { get; } = cueIndex;

        /// <summary>
        ///     The file IDs (wem source IDs) associated with this cue.
        /// </summary>
        public IReadOnlySet<uint> FileIds => _fileIds;

        internal void AddFileId(uint fileId)
        {
            _fileIds.Add(fileId);
        }
    }

#region Resolution Logic

    /// <summary>
    ///     Resolves sound file IDs for a HIRC item, traversing the Event → Action → Sound → FileId chain.
    ///     Supports cross-bank references via the bankLookup function.
    ///     Uses pattern matching for type-safe dispatch to specific handlers.
    /// </summary>
    /// <param name="bankLookup">Function to lookup soundbanks by ID for cross-bank references.</param>
    /// <param name="currentBank">The current soundbank being processed.</param>
    /// <param name="item">The HIRC item to resolve.</param>
    /// <returns>Enumerable of resolved sound file IDs.</returns>
    private static IEnumerable<uint> ResolveSoundFileIds(
        Func<uint, SoundBank?> bankLookup,
        SoundBank currentBank,
        HircItem item)
    {
        return item switch
        {
            EventItem eventItem => ResolveEventFileIds(bankLookup, currentBank, eventItem),
            ActionItem actionItem => ResolveActionFileIds(bankLookup, currentBank, actionItem),
            RanSeqCntrItem containerItem => ResolveRanSeqCntrFileIds(bankLookup, currentBank, containerItem),
            SoundItem soundItem => ResolveSoundItemFileIds(soundItem),
            _ => []
        };
    }

    /// <summary>
    ///     Resolves file IDs for an Event item by resolving all its actions.
    /// </summary>
    private static IEnumerable<uint> ResolveEventFileIds(
        Func<uint, SoundBank?> bankLookup,
        SoundBank currentBank,
        EventItem eventItem)
    {
        foreach (var actionId in eventItem.Values.Actions)
        {
            var actionItem = currentBank.HircChunk?.GetItemById(actionId);

            if (actionItem is null) continue;

            foreach (var fileId in ResolveSoundFileIds(bankLookup, currentBank, actionItem))
            {
                yield return fileId;
            }
        }
    }

    /// <summary>
    ///     Resolves file IDs for an Action item, handling cross-bank references for Play actions.
    /// </summary>
    private static IEnumerable<uint> ResolveActionFileIds(
        Func<uint, SoundBank?> bankLookup,
        SoundBank currentBank,
        ActionItem actionItem)
    {
        if (actionItem.ActionType != ActionType.Play)
        {
            yield break;
        }

        var playActionParams = actionItem.Values.PlayActionParams;

        if (playActionParams is null)
        {
            yield break;
        }

        var targetBankId = playActionParams.FileId;
        var targetItemId = actionItem.Values.Ext;

        // Determine which bank to resolve from
        SoundBank targetBank;

        if (targetBankId == currentBank.SoundbankId)
        {
            // Same bank reference
            targetBank = currentBank;
        }
        else
        {
            // Cross-bank reference - lookup the target bank
            var lookedUpBank = bankLookup(targetBankId);

            if (lookedUpBank?.HircChunk is null)
            {
                // Target bank not found or has no HIRC chunk, skip
                yield break;
            }

            targetBank = lookedUpBank;
        }

        // Find the target item in the target bank
        var targetItem = targetBank.HircChunk?.GetItemById(targetItemId);

        if (targetItem is null)
        {
            yield break;
        }

        // Recursively resolve in the target bank
        foreach (var fileId in ResolveSoundFileIds(bankLookup, targetBank, targetItem))
        {
            yield return fileId;
        }
    }

    /// <summary>
    ///     Resolves file IDs for a RanSeqCntr (Random/Sequence Container) by resolving all its children.
    /// </summary>
    private static IEnumerable<uint> ResolveRanSeqCntrFileIds(
        Func<uint, SoundBank?> bankLookup,
        SoundBank currentBank,
        RanSeqCntrItem containerItem)
    {
        foreach (var childId in containerItem.Values.Children.ChildIds)
        {
            var childItem = currentBank.HircChunk?.GetItemById(childId);

            if (childItem is null) continue;

            foreach (var fileId in ResolveSoundFileIds(bankLookup, currentBank, childItem))
            {
                yield return fileId;
            }
        }
    }

    /// <summary>
    ///     Resolves file IDs for a Sound item, extracting the source ID if it's embedded in the bank.
    /// </summary>
    private static IEnumerable<uint> ResolveSoundItemFileIds(SoundItem soundItem)
    {
        if (soundItem.Values.BankSourceData.StreamType == StreamType.DataBnk)
        {
            yield return soundItem.Values.BankSourceData.MediaInformation.SourceId;
        }
    }

#endregion
}
