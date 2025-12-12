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
    public HircItem? GetItem(uint id) => Items[id];

    /// <summary>
    ///     Gets a HIRC item by ID and casts to the specified type.
    /// </summary>
    public T? GetItem<T>(uint id) where T : HircItem => Items.Get<T>(id);

    /// <summary>
    ///     Gets embedded media by source ID.
    /// </summary>
    public byte[]? GetMedia(uint sourceId) => Media[sourceId];

    #endregion

    #region Constructors

    /// <summary>
    ///     Creates a new empty sound bank.
    /// </summary>
    public SoundBank()
    {
    }

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
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
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
        return Parse(File.ReadAllBytes(path));
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

    [Obsolete("Use Items collection instead. This property will be removed in a future version.")]
    public HircChunk? HircChunk { get; private set; }

    [Obsolete("Use Media collection instead. This property will be removed in a future version.")]
    public DataChunk? DataChunk { get; private set; }

    [Obsolete("Access properties directly on SoundBank. This property will be removed in a future version.")]
    public BankHeaderChunk? BankHeaderChunk { get; private set; }

    [Obsolete("This property will be removed in a future version.")]
    public MediaIndexChunk? MediaIndexChunk { get; private set; }

    [Obsolete("This property will be removed in a future version.")]
    public CustomPlatformChunk? PlatformChunk { get; private set; }

    [Obsolete("This property will be removed in a future version.")]
    public EnvSettingsChunk? EnvSettingsChunk { get; private set; }

    [Obsolete("Use SoundBank.Parse() instead.")]
    public bool Read(BinaryReader reader) => ReadInternal(reader);

    // Legacy compatibility properties
    [Obsolete("Use Id property instead.")]
    public uint? SoundbankId => IsValid ? Id : null;

    [Obsolete("Use Media.Count > 0 instead.")]
    public bool IsMediaLoaded => Media.Count > 0;

    [Obsolete("Use Items.Count > 0 instead.")]
    public bool IsHircLoaded => Items.Count > 0;

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
                    return true;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to parse soundbank");
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

        if (chunk.Tag == BnkChunkIds.BankHeaderChunkId)
        {
            var bankHeaderChunk = new BankHeaderChunk();

            if (!bankHeaderChunk.Read(this, reader, chunk.Size))
            {
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
        }
        else if (chunk.Tag == BnkChunkIds.BankDataIndexChunkId)
        {
            var mediaIndexChunk = new MediaIndexChunk();

            if (!mediaIndexChunk.Read(this, reader, chunk.Size))
            {
                return false;
            }

            MediaIndexChunk = mediaIndexChunk;
        }
        else if (chunk.Tag == BnkChunkIds.BankDataChunkId)
        {
            var dataChunk = new DataChunk();

            if (!dataChunk.Read(this, reader, chunk.Size))
            {
                return false;
            }

            DataChunk = dataChunk;

            // Populate Media collection from DataChunk
            if (dataChunk.Data is not null)
            {
                Media.AddRange(dataChunk.Data
                    .Select(e => new KeyValuePair<uint, byte[]>(e.Id, e.Data)));
            }
        }
        else if (chunk.Tag == BnkChunkIds.BankHierarchyChunkId)
        {
            var hircChunk = new HircChunk();

            if (!hircChunk.Read(this, reader, chunk.Size))
            {
                return false;
            }

            HircChunk = hircChunk;

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

            if (uiType != BnkStringType.Bank)
            {
                return false;
            }

            var numberOfStrings = reader.ReadUInt32();

            for (var i = 0; i < numberOfStrings; ++i)
            {
                var bankId = reader.ReadUInt32();
                var stringSize = reader.ReadByte();
                var stringBuffer = reader.ReadBytes(stringSize);
                var str = Encoding.ASCII.GetString(stringBuffer);

                // TODO: Store string map
            }

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
                return false;
            }
        }
        else if (chunk.Tag == BnkChunkIds.BankPlatChunkId)
        {
            var platform = new CustomPlatformChunk();

            if (!platform.Read(this, reader, chunk.Size))
            {
                return false;
            }
        }
        else
        {
            var strTag = chunk.MagicString;

            return false;
        }

        var expectedPosition = baseOffset + chunk.Size;

        if (reader.BaseStream.Position != expectedPosition)
        {
            Log.Warn(
                "Sub-chunk read position mismatch for chunk {0}. Expected {1}, got {2}.",
                chunk.MagicString,
                expectedPosition,
                reader.BaseStream.Position);

            reader.BaseStream.Position = expectedPosition;

            return false;
        }

        return true;
    }

    private class SubChunk
    {
        public required uint Tag { get; init; }
        public required uint Size { get; init; }

        public string MagicString => Encoding.ASCII.GetString(BitConverter.GetBytes(Tag));

        public static SubChunk Read(BinaryReader reader)
        {
            var tag = reader.ReadUInt32();
            var size = reader.ReadUInt32();

            return new SubChunk { Tag = tag, Size = size };
        }
    }

    #endregion
}
