using System.Text;

using SoundsUnpack.WWise.Bank;
using SoundsUnpack.WWise.Chunks;
using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise;

public class SoundBank
{
    public uint? SoundbankId
    {
        get => BankHeaderChunk?.SoundBankId;
        set
        {
            if (BankHeaderChunk is not null)
            {
                BankHeaderChunk.SoundBankId = value;
            }
            else
            {
                throw new InvalidOperationException("BankHeaderChunk is not initialized");
            }
        }
    }

    public uint? LanguageId
    {
        get => BankHeaderChunk?.LanguageId;
        set
        {
            if (BankHeaderChunk is not null)
            {
                BankHeaderChunk.LanguageId = value;
            }
            else
            {
                throw new InvalidOperationException("BankHeaderChunk is not initialized");
            }
        }
    }

    public uint? FeedbackInBank
    {
        get => BankHeaderChunk?.FeedbackInBank;
        set
        {
            if (BankHeaderChunk is not null)
            {
                BankHeaderChunk.FeedbackInBank = value;
            }
            else
            {
                throw new InvalidOperationException("BankHeaderChunk is not initialized");
            }
        }
    }

    public uint? ProjectId
    {
        get => BankHeaderChunk?.ProjectId;
        set
        {
            if (BankHeaderChunk is not null)
            {
                BankHeaderChunk.FeedbackInBank = value;
            }
            else
            {
                throw new InvalidOperationException("BankHeaderChunk is not initialized");
            }
        }
    }

    public bool IsValid =>
        BankHeaderChunk?.IsValid == true;

    public bool IsMediaLoaded => IsValid && DataChunk?.Data is not null;
    public bool IsHircLoaded => IsValid && HircChunk?.Items is not null;

    public BankHeaderChunk? BankHeaderChunk { get; private set; }
    public CustomPlatformChunk? PlatformChunk { get; private set; }
    public EnvSettingsChunk? EnvSettingsChunk { get; private set; }
    public MediaIndexChunk? MediaIndexChunk { get; private set; }
    public DataChunk? DataChunk { get; private set; }
    public HircChunk? HircChunk { get; private set; }

    public bool Read(BinaryReader reader)
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
        }
        else if (chunk.Tag == BnkChunkIds.BankHierarchyChunkId)
        {
            var hircChunk = new HircChunk();

            if (!hircChunk.Read(this, reader, chunk.Size))
            {
                return false;
            }

            HircChunk = hircChunk;
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
}
