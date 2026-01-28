using System.Text;

using PckTool.Core.WWise.Bnk.Bank;
using PckTool.Core.WWise.Bnk.Chunks;
using PckTool.Core.WWise.Bnk.Enums;
using PckTool.Core.WWise.Bnk.Structs;

namespace PckTool.Core.WWise.Bnk;

/// <summary>
///     Represents a WWise SoundBank (.bnk) file.
///     Provides parsing, modification, and (future) serialization of sound banks.
/// </summary>
public class SoundBank
{
#region Properties

    /// <summary>
    ///     The unique ID of this sound bank.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    ///     The language ID of this sound bank.
    /// </summary>
    public uint LanguageId { get; set; }

    /// <summary>
    ///     The WWise version this bank was created with.
    /// </summary>
    public uint Version { get; set; } = 0x71; // Default WWise version

    /// <summary>
    ///     The project ID.
    /// </summary>
    public uint ProjectId { get; set; }

    /// <summary>
    ///     Feedback in bank flag.
    /// </summary>
    public uint FeedbackInBank { get; set; }

    /// <summary>
    ///     Returns true if the bank was successfully parsed.
    /// </summary>
    public bool IsValid { get; private set; }

#endregion

#region Collections

    /// <summary>
    ///     All HIRC items in this sound bank.
    /// </summary>
    public HircCollection Items { get; } = new();

    /// <summary>
    ///     Embedded media (WEM files) in this sound bank.
    /// </summary>
    public MediaCollection Media { get; } = new();

#endregion

#region Typed Accessors

    /// <summary>
    ///     All Event items in this sound bank.
    /// </summary>
    public IEnumerable<EventItem> Events => Items.OfType<EventItem>();

    /// <summary>
    ///     All Sound items in this sound bank.
    /// </summary>
    public IEnumerable<SoundItem> Sounds => Items.OfType<SoundItem>();

    /// <summary>
    ///     All Action items in this sound bank.
    /// </summary>
    public IEnumerable<ActionItem> Actions => Items.OfType<ActionItem>();

    /// <summary>
    ///     All RanSeqCntr (Random/Sequence Container) items in this sound bank.
    /// </summary>
    public IEnumerable<RanSeqCntrItem> RanSeqContainers => Items.OfType<RanSeqCntrItem>();

    /// <summary>
    ///     All MusicSegment items in this sound bank.
    /// </summary>
    public IEnumerable<MusicSegmentItem> MusicSegments => Items.OfType<MusicSegmentItem>();

    /// <summary>
    ///     All MusicTrack items in this sound bank.
    /// </summary>
    public IEnumerable<MusicTrackItem> MusicTracks => Items.OfType<MusicTrackItem>();

    /// <summary>
    ///     All MusicSwitch items in this sound bank.
    /// </summary>
    public IEnumerable<MusicSwitchItem> MusicSwitches => Items.OfType<MusicSwitchItem>();

    /// <summary>
    ///     All MusicRanSeq items in this sound bank.
    /// </summary>
    public IEnumerable<MusicRanSeqItem> MusicRanSeqs => Items.OfType<MusicRanSeqItem>();

#endregion

#region Lookup Methods

    /// <summary>
    ///     Gets a HIRC item by ID.
    /// </summary>
    public HircItem? GetItem(uint id)
    {
        return Items[id];
    }

    /// <summary>
    ///     Gets a HIRC item by ID and casts to the specified type.
    /// </summary>
    public T? GetItem<T>(uint id) where T : HircItem
    {
        return Items.Get<T>(id);
    }

    /// <summary>
    ///     Gets embedded media by source ID.
    /// </summary>
    public byte[]? GetMedia(uint sourceId)
    {
        return Media[sourceId];
    }

    /// <summary>
    ///     Gets all MediaInformation references for a given source ID.
    ///     This includes Sound items, MusicTrack sources, and FeedbackNode sources.
    /// </summary>
    /// <param name="sourceId">The WEM source ID to find references for.</param>
    /// <returns>All MediaInformation objects that reference this source ID.</returns>
    public IEnumerable<MediaInformation> GetMediaReferences(uint sourceId)
    {
        // Check Sound items
        foreach (var sound in Sounds)
        {
            if (sound.Values.BankSourceData.MediaInformation.SourceId == sourceId)
            {
                yield return sound.Values.BankSourceData.MediaInformation;
            }
        }

        // Check MusicTrack items (can have multiple sources)
        foreach (var track in MusicTracks)
        {
            foreach (var source in track.Values.Sources)
            {
                if (source.MediaInformation.SourceId == sourceId)
                {
                    yield return source.MediaInformation;
                }
            }
        }

        // Check FeedbackNode items
        foreach (var item in Items.OfType<FeedbackNodeItem>())
        {
            foreach (var source in item.Values.Sources)
            {
                if (source.BankSourceData.MediaInformation.SourceId == sourceId)
                {
                    yield return source.BankSourceData.MediaInformation;
                }
            }
        }
    }

    /// <summary>
    ///     Gets all HIRC items that reference a given source ID.
    /// </summary>
    /// <param name="sourceId">The WEM source ID to find references for.</param>
    /// <returns>All HIRC items that reference this source ID.</returns>
    public IEnumerable<HircItem> GetItemsBySourceId(uint sourceId)
    {
        // Check Sound items
        foreach (var sound in Sounds)
        {
            if (sound.Values.BankSourceData.MediaInformation.SourceId == sourceId)
            {
                yield return sound;
            }
        }

        // Check MusicTrack items
        foreach (var track in MusicTracks)
        {
            if (track.Values.Sources.Any(s => s.MediaInformation.SourceId == sourceId))
            {
                yield return track;
            }
        }

        // Check FeedbackNode items
        foreach (var item in Items.OfType<FeedbackNodeItem>())
        {
            if (item.Values.Sources.Any(s => s.BankSourceData.MediaInformation.SourceId == sourceId))
            {
                yield return item;
            }
        }
    }

#endregion

#region WEM Replacement

    /// <summary>
    ///     Replaces embedded WEM data and updates all HIRC references to reflect the new size.
    /// </summary>
    /// <param name="sourceId">The WEM source ID to replace.</param>
    /// <param name="newData">The new WEM data.</param>
    /// <param name="updateHircSizes">If true, updates InMemoryMediaSize in all HIRC items that reference this WEM.</param>
    /// <returns>The number of HIRC references that were updated.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the source ID doesn't exist in the Media collection.</exception>
    public int ReplaceWem(uint sourceId, byte[] newData, bool updateHircSizes = true)
    {
        if (!Media.Contains(sourceId))
        {
            throw new KeyNotFoundException($"WEM with source ID 0x{sourceId:X8} not found in embedded media.");
        }

        // Replace the media data
        Media[sourceId] = newData;

        if (!updateHircSizes)
        {
            return 0;
        }

        // Update all HIRC references
        var updatedCount = 0;

        foreach (var mediaInfo in GetMediaReferences(sourceId))
        {
            mediaInfo.InMemoryMediaSize = (uint) newData.Length;
            updatedCount++;
        }

        return updatedCount;
    }

    /// <summary>
    ///     Replaces embedded WEM data from a file and updates all HIRC references.
    /// </summary>
    /// <param name="sourceId">The WEM source ID to replace.</param>
    /// <param name="filePath">Path to the new WEM file.</param>
    /// <param name="updateHircSizes">If true, updates InMemoryMediaSize in all HIRC items that reference this WEM.</param>
    /// <returns>The number of HIRC references that were updated.</returns>
    public int ReplaceWemFromFile(uint sourceId, string filePath, bool updateHircSizes = true)
    {
        var newData = File.ReadAllBytes(filePath);

        return ReplaceWem(sourceId, newData, updateHircSizes);
    }

    /// <summary>
    ///     Adds a new embedded WEM or replaces an existing one, updating HIRC references if it exists.
    /// </summary>
    /// <param name="sourceId">The WEM source ID.</param>
    /// <param name="data">The WEM data.</param>
    /// <param name="updateHircSizes">If true and the WEM exists, updates InMemoryMediaSize in all HIRC items.</param>
    /// <returns>The number of HIRC references that were updated (0 if this was a new WEM).</returns>
    public int SetWem(uint sourceId, byte[] data, bool updateHircSizes = true)
    {
        var exists = Media.Contains(sourceId);
        Media[sourceId] = data;

        if (!exists || !updateHircSizes)
        {
            return 0;
        }

        var updatedCount = 0;

        foreach (var mediaInfo in GetMediaReferences(sourceId))
        {
            mediaInfo.InMemoryMediaSize = (uint) data.Length;
            updatedCount++;
        }

        return updatedCount;
    }

    /// <summary>
    ///     Updates the InMemoryMediaSize for all HIRC items referencing a source ID.
    ///     Useful when you've modified media externally and need to sync HIRC.
    /// </summary>
    /// <param name="sourceId">The WEM source ID.</param>
    /// <param name="newSize">The new size to set.</param>
    /// <returns>The number of HIRC references that were updated.</returns>
    public int UpdateMediaSize(uint sourceId, uint newSize)
    {
        var updatedCount = 0;

        foreach (var mediaInfo in GetMediaReferences(sourceId))
        {
            mediaInfo.InMemoryMediaSize = newSize;
            updatedCount++;
        }

        return updatedCount;
    }

#endregion

#region Constructors

    /// <summary>
    ///     Creates a new empty sound bank.
    /// </summary>
    public SoundBank() { }

    /// <summary>
    ///     Creates a new sound bank with the specified ID.
    /// </summary>
    public SoundBank(uint id, uint languageId = 0)
    {
        Id = id;
        LanguageId = languageId;
        IsValid = true;
    }

#endregion

#region Parse Methods

    /// <summary>
    ///     Parses a sound bank from a byte array.
    /// </summary>
    public static SoundBank? Parse(byte[] data)
    {
        using var stream = new MemoryStream(data);

        return Parse(stream);
    }

    /// <summary>
    ///     Parses a sound bank from a stream.
    /// </summary>
    public static SoundBank? Parse(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        var bank = new SoundBank();

        if (bank.ReadInternal(reader))
        {
            return bank;
        }

        return null;
    }

    /// <summary>
    ///     Parses a sound bank from a file.
    /// </summary>
    public static SoundBank? Load(string path)
    {
        using var stream = File.OpenRead(path);

        return Parse(stream);
    }

#endregion

#region Write Methods

    /// <summary>
    ///     Serializes the sound bank to a byte array.
    /// </summary>
    public byte[] ToByteArray()
    {
        using var stream = new MemoryStream();
        Write(stream);

        return stream.ToArray();
    }

    /// <summary>
    ///     Writes the sound bank to a stream.
    /// </summary>
    public void Write(Stream stream)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        // Prepare chunks from current state
        PrepareChunksForWrite();

        // Write BKHD chunk (always required)
        BankHeaderChunk?.Write(this, writer);

        // Write DIDX and DATA chunks (only if there's embedded media)
        if (Media.Count > 0 && MediaIndexChunk is not null && DataChunk is not null)
        {
            // DIDX must be written before DATA
            MediaIndexChunk.Write(this, writer);
            DataChunk.Write(this, writer);
        }

        // Write HIRC chunk (only if there are items)
        if (Items.Count > 0 && HircChunk is not null)
        {
            HircChunk.Write(this, writer);
        }

        // Write optional chunks if they exist
        StringMapChunk?.Write(this, writer);
        StateManagerChunk?.Write(this, writer);
        FxParamsChunk?.Write(this, writer);
        EnvSettingsChunk?.Write(this, writer);
        PlatformChunk?.Write(this, writer);
        InitChunk?.Write(this, writer);
    }

    /// <summary>
    ///     Saves the sound bank to a file.
    /// </summary>
    public void Save(string path)
    {
        using var stream = File.Create(path);
        Write(stream);
    }

    /// <summary>
    ///     Prepares internal chunks from the public collections for writing.
    /// </summary>
    private void PrepareChunksForWrite()
    {
        // Create or update BankHeaderChunk
        BankHeaderChunk ??= new BankHeaderChunk();
        BankHeaderChunk.BankGeneratorVersion = Version;
        BankHeaderChunk.SoundBankId = Id;
        BankHeaderChunk.LanguageId = LanguageId;
        BankHeaderChunk.FeedbackInBank = FeedbackInBank;
        BankHeaderChunk.ProjectId = ProjectId;

        // Create or update HircChunk from Items collection
        if (Items.Count > 0)
        {
            HircChunk ??= new HircChunk();

            // Use reflection to set the private Items property, or we need to modify HircChunk
            // For now, we'll create a new approach - sync the Items list
            SyncHircChunkItems();
        }

        // Create or update MediaIndexChunk and DataChunk from Media collection
        if (Media.Count > 0)
        {
            SyncMediaChunks();
        }
    }

    /// <summary>
    ///     Synchronizes the HircChunk.Items with the public Items collection.
    /// </summary>
    private void SyncHircChunkItems()
    {
        // Create a new HircChunk with the current items
        var hircChunk = new HircChunk();

        // We need to set the Items property - but it's private set
        // Let's use a different approach: create a method in HircChunk or use the existing chunk
        // For now, we'll leverage the fact that HircChunk.WriteInternal reads from Items property
        // We need to make the Items property settable or provide an alternative

        // Workaround: HircChunk stores items internally, so we need to ensure it has the right items
        // If HircChunk was loaded from parsing, it already has the items
        // If we're creating from scratch, we need to populate it

        if (HircChunk is null)
        {
            HircChunk = hircChunk;
        }

        // The HircChunk needs to be updated to support setting Items
        // For now, we'll add a SetItems method to HircChunk
        HircChunk.SetItems(Items.ToList());
    }

    /// <summary>
    ///     Synchronizes the MediaIndexChunk and DataChunk with the public Media collection.
    /// </summary>
    private void SyncMediaChunks()
    {
        // If we already have MediaIndexChunk and DataChunk from parsing, preserve the original
        // offsets to maintain alignment/padding. Only create new headers if we're building from scratch.
        if (MediaIndexChunk?.LoadedMedia is not null && DataChunk?.Data is not null)
        {
            // Already have parsed chunks - the DataChunk.WriteInternal will use the existing
            // headers with their original offsets to preserve padding
            return;
        }

        // Create new MediaIndexChunk with headers (no existing chunks - building from scratch)
        var headers = new List<MediaHeader>();
        var dataEntries = new List<DataChunk.MediaIndexEntry>();

        uint currentOffset = 0;

        foreach (var kvp in Media)
        {
            var header = new MediaHeader { Id = kvp.Key, Offset = currentOffset, Size = (uint) kvp.Value.Length };
            headers.Add(header);

            var dataEntry = new DataChunk.MediaIndexEntry { Id = kvp.Key, Data = kvp.Value };
            dataEntries.Add(dataEntry);

            currentOffset += header.Size;
        }

        // Create MediaIndexChunk
        MediaIndexChunk ??= new MediaIndexChunk();
        MediaIndexChunk.SetLoadedMedia(headers);

        // Create DataChunk
        DataChunk ??= new DataChunk();
        DataChunk.SetData(dataEntries);
    }

#endregion

#region Legacy Compatibility

    // These properties maintain backwards compatibility with existing code
    // that accesses chunks directly. They will be removed in a future version.

    // Internal chunk storage (used during parsing)
    internal HircChunk? HircChunk { get; private set; }
    internal DataChunk? DataChunk { get; private set; }
    internal BankHeaderChunk? BankHeaderChunk { get; private set; }
    internal MediaIndexChunk? MediaIndexChunk { get; private set; }
    internal CustomPlatformChunk? PlatformChunk { get; private set; }
    internal EnvSettingsChunk? EnvSettingsChunk { get; private set; }
    internal StringMapChunk? StringMapChunk { get; private set; }
    internal StateManagerChunk? StateManagerChunk { get; private set; }
    internal FxParamsChunk? FxParamsChunk { get; private set; }
    internal InitChunk? InitChunk { get; private set; }

#endregion

#region Internal Parsing

    private bool ReadInternal(BinaryReader reader)
    {
        try
        {
            while (ProcessSubChunk(reader, out var completed))
            {
                if (completed)
                {
                    Log.Debug(
                        "Successfully parsed soundbank 0x{0:X8} (version 0x{1:X}, {2} items, {3} media)",
                        Id,
                        Version,
                        Items.Count,
                        Media.Count);

                    return true;
                }
            }

            Log.Error(
                "ProcessSubChunk returned false for soundbank 0x{0:X8} at position {1}/{2}",
                Id,
                reader.BaseStream.Position,
                reader.BaseStream.Length);
        }
        catch (Exception e)
        {
            Log.Error(
                e,
                $"Failed to parse soundbank 0x{Id:X8} at position {reader.BaseStream.Position}/{reader.BaseStream.Length}");
        }

        return false;
    }

    private bool ProcessSubChunk(BinaryReader reader, out bool completed)
    {
        if (reader.BaseStream.Position >= reader.BaseStream.Length)
        {
            completed = true;

            return true;
        }

        completed = false;

        var chunk = SubChunk.Read(reader);
        var baseOffset = reader.BaseStream.Position;

        Log.Trace(
            "Processing chunk '{0}' (0x{1:X8}), size: {2}, at offset: {3}",
            chunk.MagicString,
            chunk.Tag,
            chunk.Size,
            baseOffset - 8);

        if (chunk.Tag == BnkChunkIds.BankHeaderChunkId)
        {
            var bankHeaderChunk = new BankHeaderChunk();

            if (!bankHeaderChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read BKHD chunk for bank at offset {0}", baseOffset);

                return false;
            }

            BankHeaderChunk = bankHeaderChunk;

            // Populate new properties from header
            Id = bankHeaderChunk.SoundBankId ?? 0;
            LanguageId = bankHeaderChunk.LanguageId ?? 0;
            Version = bankHeaderChunk.BankGeneratorVersion ?? 0x71;
            ProjectId = bankHeaderChunk.ProjectId ?? 0;
            FeedbackInBank = bankHeaderChunk.FeedbackInBank ?? 0;
            IsValid = bankHeaderChunk.IsValid;

            Log.Trace("  BKHD: Bank ID=0x{0:X8}, Version=0x{1:X}, Lang={2}", Id, Version, LanguageId);
        }
        else if (chunk.Tag == BnkChunkIds.BankDataIndexChunkId)
        {
            var mediaIndexChunk = new MediaIndexChunk();

            if (!mediaIndexChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read DIDX chunk for bank 0x{0:X8}", Id);

                return false;
            }

            MediaIndexChunk = mediaIndexChunk;
            Log.Trace("  DIDX: {0} media entries", mediaIndexChunk.LoadedMedia?.Count ?? 0);
        }
        else if (chunk.Tag == BnkChunkIds.BankDataChunkId)
        {
            var dataChunk = new DataChunk();

            if (!dataChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read DATA chunk for bank 0x{0:X8}", Id);

                return false;
            }

            DataChunk = dataChunk;

            // Populate Media collection from DataChunk
            if (dataChunk.Data is not null)
            {
                Media.AddRange(
                    dataChunk.Data
                             .Select(e => new KeyValuePair<uint, byte[]>(e.Id, e.Data)));
            }
        }
        else if (chunk.Tag == BnkChunkIds.BankHierarchyChunkId)
        {
            var hircChunk = new HircChunk();

            if (!hircChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read HIRC chunk for bank 0x{0:X8}", Id);

                return false;
            }

            HircChunk = hircChunk;
            Log.Trace("  HIRC: {0} items", hircChunk.Items?.Count ?? 0);

            // Populate Items collection from HircChunk
            if (hircChunk.Items is not null)
            {
                Items.AddRange(hircChunk.Items);
            }
        }
        else if (chunk.Tag == BnkChunkIds.BankStrMapChunkId)
        {
            var stringMapChunk = new StringMapChunk();

            if (!stringMapChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read STID chunk for bank 0x{0:X8}", Id);

                return false;
            }

            StringMapChunk = stringMapChunk;
            Log.Trace("  STID: {0} bank names", stringMapChunk.BankNames.Count);
        }
        else if (chunk.Tag == BnkChunkIds.BankStateMgrChunkId)
        {
            var stateManagerChunk = new StateManagerChunk();

            if (!stateManagerChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read STMG chunk for bank 0x{0:X8}", Id);

                return false;
            }

            StateManagerChunk = stateManagerChunk;
            Log.Trace("  STMG: Global settings loaded ({0} bytes)", chunk.Size);
        }
        else if (chunk.Tag == BnkChunkIds.BankFxParamsChunkId)
        {
            var fxParamsChunk = new FxParamsChunk();

            if (!fxParamsChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read FXPR chunk for bank 0x{0:X8}", Id);

                return false;
            }

            FxParamsChunk = fxParamsChunk;
            Log.Trace("  FXPR: FX params loaded ({0} bytes)", chunk.Size);
        }
        else if (chunk.Tag == BnkChunkIds.BankEnvSettingChunkId)
        {
            var envSettingsChunk = new EnvSettingsChunk();

            if (!envSettingsChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read ENVS chunk for bank 0x{0:X8}", Id);

                return false;
            }

            EnvSettingsChunk = envSettingsChunk;
            Log.Trace("  ENVS: Environment settings loaded");
        }
        else if (chunk.Tag == BnkChunkIds.BankPlatChunkId)
        {
            var platform = new CustomPlatformChunk();

            if (!platform.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read PLAT chunk for bank 0x{0:X8}", Id);

                return false;
            }

            PlatformChunk = platform;
            Log.Trace("  PLAT: Platform chunk loaded");
        }
        else if (chunk.Tag == BnkChunkIds.BankInitChunkId)
        {
            var initChunk = new InitChunk();

            if (!initChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read INIT chunk for bank 0x{0:X8}", Id);

                return false;
            }

            InitChunk = initChunk;
            Log.Trace("  INIT: Plugin info loaded ({0} bytes)", chunk.Size);
        }
        else
        {
            // Unknown chunk type - skip it instead of failing
            // This allows parsing banks with newer/unknown chunk types
            var strTag = chunk.MagicString;
            Log.Warn(
                "Skipping unknown chunk type: {0} (0x{1:X8}), size: {2} in bank 0x{3:X8}",
                strTag,
                chunk.Tag,
                chunk.Size,
                Id);

            reader.BaseStream.Position += chunk.Size;
        }

        var expectedPosition = baseOffset + chunk.Size;

        if (reader.BaseStream.Position != expectedPosition)
        {
            Log.Error(
                "Sub-chunk read position mismatch for chunk {0} in bank 0x{1:X8}. Expected {2}, got {3}. Delta: {4} bytes.",
                chunk.MagicString,
                Id,
                expectedPosition,
                reader.BaseStream.Position,
                reader.BaseStream.Position - expectedPosition);

            reader.BaseStream.Position = expectedPosition;

            return false;
        }

        return true;
    }

    private class SubChunk
    {
        public required uint Tag { get; init; }
        public required uint Size { get; init; }

        public string MagicString =>
            new(
                new[]
                {
                    (char) (Tag & 0xFF),
                    (char) ((Tag >> 8) & 0xFF),
                    (char) ((Tag >> 16) & 0xFF),
                    (char) ((Tag >> 24) & 0xFF)
                });

        public static SubChunk Read(BinaryReader reader)
        {
            var tag = reader.ReadUInt32();
            var size = reader.ReadUInt32();

            return new SubChunk { Tag = tag, Size = size };
        }
    }

#endregion
}
