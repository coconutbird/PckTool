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
        throw new NotImplementedException("SoundBank serialization is not yet implemented.");
    }

    /// <summary>
    ///     Writes the sound bank to a stream.
    /// </summary>
    public void Write(Stream stream)
    {
        throw new NotImplementedException("SoundBank serialization is not yet implemented.");
    }

    /// <summary>
    ///     Saves the sound bank to a file.
    /// </summary>
    public void Save(string path)
    {
        throw new NotImplementedException("SoundBank serialization is not yet implemented.");
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
            var position = reader.BaseStream.Position;

            var uiType = (BnkStringType) reader.ReadUInt32();

            if (uiType == BnkStringType.Bank)
            {
                var numberOfStrings = reader.ReadUInt32();

                for (var i = 0; i < numberOfStrings; ++i)
                {
                    var bankId = reader.ReadUInt32();
                    var stringSize = reader.ReadByte();
                    var stringBuffer = reader.ReadBytes(stringSize);
                    var str = Encoding.ASCII.GetString(stringBuffer);

                    // TODO: Store string map
                }
            }

            // Skip unsupported string types (there are multiple types like 1/2/3/4/5/7/8/9/11)
            // We only care about type 1 (Bank) for now

            reader.BaseStream.Position = position + chunk.Size;
        }
        else if (chunk.Tag == BnkChunkIds.BankStateMgrChunkId)
        {
            var position = reader.BaseStream.Position;

            if (chunk.Size > 0)
            {
                var volumeThreshold = reader.ReadSingle();
                var maxNumberOfVoicesLimitInternal = reader.ReadUInt16();
                var numberOfStateGroups = reader.ReadUInt32();

                var mapTransitions = new List<SubHircSection>();

                for (var i = 0; i < numberOfStateGroups; ++i)
                {
                    var stateGroupId = reader.ReadUInt32();
                    var defaultTransitionTime = reader.ReadInt32();
                    var numberOfTransitions = reader.ReadUInt32();

                    for (var j = 0; j < numberOfTransitions; ++j)
                    {
                        var stateType = reader.ReadUInt32();
                        var group = new SubHircSection
                        {
                            Type = (HircType) reader.ReadByte(), SectionSize = reader.ReadUInt32()
                        };

                        mapTransitions.Add(group);
                    }
                }

                var numberOfSwitchGroups = reader.ReadUInt32();

                for (var i = 0; i < numberOfSwitchGroups; ++i)
                {
                    var switchGroupId = reader.ReadUInt32();
                    var rtpcId = reader.ReadUInt32();

                    var rtpcGraphPointIntegers = new List<RtpcGraphPointBase<uint>>();
                    var rtpcGraphPointIntegerCount = reader.ReadUInt32();

                    for (var j = 0; j < rtpcGraphPointIntegerCount; ++j)
                    {
                        var point = new RtpcGraphPointBase<uint>();

                        if (!point.Read(reader))
                        {
                            return false;
                        }

                        rtpcGraphPointIntegers.Add(point);
                    }
                }

                var numberOfParams = reader.ReadUInt32();

                for (var i = 0; i < numberOfParams; ++i)
                {
                    var rtpcId = reader.ReadUInt32();
                    var value = reader.ReadSingle();
                    var rampType = reader.ReadUInt32();
                    var rampUp = reader.ReadSingle();
                    var rampDown = reader.ReadSingle();
                    var bindToBuiltInParameter = reader.ReadByte();
                }
            }

            reader.BaseStream.Position = position + chunk.Size;
        }
        else if (chunk.Tag == BnkChunkIds.BankFxParamsChunkId)
        {
            var position = reader.BaseStream.Position;

            reader.BaseStream.Position = position + chunk.Size;
        }
        else if (chunk.Tag == BnkChunkIds.BankEnvSettingChunkId)
        {
            var envSettingsChunk = new EnvSettingsChunk();

            if (!envSettingsChunk.Read(this, reader, chunk.Size))
            {
                Log.Error("Failed to read ENVS chunk for bank 0x{0:X8}", Id);

                return false;
            }

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

            Log.Trace("  PLAT: Platform chunk loaded");
        }
        else if (chunk.Tag == BnkChunkIds.BankInitChunkId)
        {
            // INIT chunk contains plugin information (version 118+)
            // Skip for now - we don't need to parse plugin details
            Log.Trace("  INIT: Skipping plugin info chunk ({0} bytes)", chunk.Size);
            reader.BaseStream.Position += chunk.Size;
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
