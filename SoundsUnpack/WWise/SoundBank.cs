using SoundsUnpack.WWise.Bank;
using SoundsUnpack.WWise.Chunks;
using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise;

public class SoundBank
{
    public uint SoundbankId { get; private set; }
    public uint LanguageId { get; private set; }
    public uint FeedbackInBank { get; private set; }
    public uint ProjectId { get; private set; }

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
            Console.WriteLine(e.Message);
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

            if (!bankHeaderChunk.Read(reader, chunk.Size))
            {
                return false;
            }

            BankHeaderChunk = bankHeaderChunk;
        }
        else if (chunk.Tag == BnkChunkIds.BankDataIndexChunkId)
        {
            var mediaIndexChunk = new MediaIndexChunk();

            if (!mediaIndexChunk.Read(reader, chunk.Size))
            {
                return false;
            }

            MediaIndexChunk = mediaIndexChunk;
        }
        else if (chunk.Tag == BnkChunkIds.BankDataChunkId)
        {
            if (MediaIndexChunk is null)
            {
                return false;
            }

            var dataChunk = new DataChunk();

            if (!dataChunk.Read(reader, MediaIndexChunk, chunk.Size))
            {
                return false;
            }

            DataChunk = dataChunk;
        }
        else if (chunk.Tag == BnkChunkIds.BankHierarchyChunkId)
        {
            var hircChunk = new HircChunk();

            if (!hircChunk.Read(reader, chunk.Size))
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
                var str = System.Text.Encoding.ASCII.GetString(stringBuffer);

                // TODO: Store string map

                continue;
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

                    if (rtpcGraphPointIntegerCount > 0)
                    {
                        for (var j = 0; j < rtpcGraphPointIntegerCount; ++j)
                        {
                            var point = new RtpcGraphPointBase<uint>
                            {
                                From = reader.ReadUInt32(),
                                To = reader.ReadUInt32(),
                                InterpolationType = (CurveInterpolation) reader.ReadUInt32()
                            };

                            rtpcGraphPointIntegers.Add(point);
                        }
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

            if (!envSettingsChunk.Read(reader, chunk.Size))
            {
                return false;
            }
        }
        else if (chunk.Tag == BnkChunkIds.BankPlatChunkId)
        {
            var platform = new CustomPlatformChunk();

            if (!platform.Read(reader, chunk.Size))
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
            Console.WriteLine(
                $"Warning: Sub-chunk read position mismatch for chunk {chunk.MagicString}. Expected {expectedPosition}, got {reader.BaseStream.Position}.");

            reader.BaseStream.Position = expectedPosition;
        }

        return true;
    }

    public class Wem
    {
        public required uint WemId { get; init; }
        public required byte[] Data { get; init; } = [];
    }

    private class SubChunk
    {
        public required uint Tag { get; init; }
        public required uint Size { get; init; }

        public string MagicString => System.Text.Encoding.ASCII.GetString(BitConverter.GetBytes(Tag));

        public static SubChunk Read(BinaryReader reader)
        {
            var tag = reader.ReadUInt32();
            var size = reader.ReadUInt32();

            return new SubChunk { Tag = tag, Size = size };
        }
    }

    private class DataIndexChunk
    {
        public required uint WemId { get; init; }
        public required uint Offset { get; init; }
        public required uint Size { get; init; }
    }

    private const uint ValidVersion = 0x71;
}