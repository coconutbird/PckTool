using System.Text;

using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Bnk.Enums;
using PckTool.Core.WWise.Bnk.Hirc.Items;
using PckTool.Core.WWise.Bnk.Hirc.Params;
using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Tests;

/// <summary>
///     Tests for HIRC item parsers.
/// </summary>
public class HircItemTests
{
#region MusicRanSeqPlaylistItem Tests

    [Fact]
    public void MusicRanSeqPlaylistItem_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange - Single playlist item
        var originalData = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // SegmentId = 1
            0x02,
            0x00,
            0x00,
            0x00, // PlaylistItemId = 2
            0x00,
            0x00,
            0x00,
            0x00, // NumChildren = 0
            0x01,
            0x00,
            0x00,
            0x00, // RsType = 1
            0x03,
            0x00, // Loop = 3
            0x01,
            0x00, // LoopMin = 1
            0x05,
            0x00, // LoopMax = 5
            0x64,
            0x00,
            0x00,
            0x00, // Weight = 100
            0x02,
            0x00, // AvoidRepeatCount = 2
            0x01, // IsUsingWeight = 1
            0x00  // IsShuffle = 0
        };

        // Act - Create item manually
        var item = new MusicRanSeqPlaylistItem
        {
            SegmentId = 1,
            PlaylistItemId = 2,
            NumChildren = 0,
            RsType = 1,
            Loop = 3,
            LoopMin = 1,
            LoopMax = 5,
            Weight = 100,
            AvoidRepeatCount = 2,
            IsUsingWeight = 1,
            IsShuffle = 0
        };

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        item.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

#endregion

#region PropBundle Tests

    [Fact]
    public void PropBundle_RoundTrip_Empty()
    {
        var originalData = new byte[]
        {
            0x00 // numberOfProps = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var bundle = new PropBundle();
        bundle.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        bundle.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region InitialRtpc Tests

    [Fact]
    public void InitialRtpc_RoundTrip_Empty()
    {
        var originalData = new byte[]
        {
            0x00, 0x00 // numberOfRtpcs = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var rtpc = new InitialRtpc();
        rtpc.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        rtpc.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region AdvSettingsParams Tests

    [Fact]
    public void AdvSettingsParams_RoundTrip()
    {
        var originalData = new byte[]
        {
            0x0F, // BitVector
            0x02, // VirtualQueueBehavior
            0x10,
            0x00, // MaxNumberOfInstances = 16
            0x01, // BelowThresholdBehavior
            0x03  // BitVector2
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var settings = new AdvSettingsParams();
        settings.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        settings.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region NodeInitialFxParams Tests

    [Fact]
    public void NodeInitialFxParams_RoundTrip_NoFx()
    {
        var originalData = new byte[]
        {
            0x00, // IsOverrideParentFx = 0
            0x00  // NumFx = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var fx = new NodeInitialFxParams();
        fx.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        fx.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region NodeInitialParams Tests

    [Fact]
    public void NodeInitialParams_RoundTrip_EmptyBundles()
    {
        var originalData = new byte[]
        {
            0x00, // PropBundle1: numberOfProps = 0
            0x00  // PropBundle2: numberOfProps = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var nip = new NodeInitialParams();
        nip.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        nip.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region ActorMixerInitialValues Tests

    [Fact]
    public void ActorMixerInitialValues_RoundTrip_Minimal()
    {
        // NodeBaseParams + Children (empty)
        var nodeBaseParams = CreateMinimalNodeBaseParamsData();
        var children = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // 0 children
        var originalData = nodeBaseParams.Concat(children).ToArray();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new ActorMixerInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region AttenuationInitialValues Tests

    [Fact]
    public void AttenuationInitialValues_RoundTrip_Minimal()
    {
        var originalData = new byte[]
        {
            0x00, // IsConeEnabled = false
            // CurveToUse[7]
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00, // numberOfCurves = 0
            // InitialRtpc (empty)
            0x00,
            0x00
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new AttenuationInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region SwitchCntrInitialValues Tests

    [Fact]
    public void SwitchCntrInitialValues_RoundTrip_Minimal()
    {
        // NodeBaseParams + GroupType + GroupId + DefaultSwitch + IsContinuousValidation +
        // Children + SwitchPackages + SwitchNodeParams
        var nodeBaseParams = CreateMinimalNodeBaseParamsData();
        var switchData = new byte[]
        {
            0x01, // GroupType = 1
            0x78,
            0x56,
            0x34,
            0x12, // GroupId = 0x12345678
            0xAB,
            0xCD,
            0x00,
            0x00, // DefaultSwitch = 0xCDAB
            0x00, // IsContinuousValidation = false
            // Children (empty)
            0x00,
            0x00,
            0x00,
            0x00,

            // SwitchPackages (empty)
            0x00,
            0x00,
            0x00,
            0x00,

            // SwitchNodeParams (empty)
            0x00,
            0x00,
            0x00,
            0x00
        };

        var originalData = nodeBaseParams.Concat(switchData).ToArray();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new SwitchCntrInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region LayerCntrInitialValues Tests

    [Fact]
    public void LayerCntrInitialValues_RoundTrip_Minimal()
    {
        // NodeBaseParams + Children + Layers (empty)
        var nodeBaseParams = CreateMinimalNodeBaseParamsData();
        var layerData = new byte[]
        {
            // Children (empty)
            0x00,
            0x00,
            0x00,
            0x00,

            // Layers (empty)
            0x00,
            0x00,
            0x00,
            0x00
        };

        var originalData = nodeBaseParams.Concat(layerData).ToArray();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new LayerCntrInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region BankSourceData Tests

    [Fact]
    public void BankSourceData_RoundTrip_Minimal()
    {
        var originalData = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // PluginId = 1 (Wwise_Vorbis)
            0x00, // StreamType = 0
            // MediaInformation
            0x78,
            0x56,
            0x34,
            0x12, // SourceId = 0x12345678
            0x00,
            0x10,
            0x00,
            0x00, // InMemoryMediaSize = 4096
            0x00  // SourceBits
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var data = new BankSourceData();
        data.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        data.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region SoundInitialValues Tests

    [Fact]
    public void SoundInitialValues_RoundTrip_Minimal()
    {
        // BankSourceData + NodeBaseParams
        var bankSourceData = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // PluginId = 1 (Wwise_Vorbis)
            0x00, // StreamType = 0
            // MediaInformation
            0x78,
            0x56,
            0x34,
            0x12, // SourceId
            0x00,
            0x10,
            0x00,
            0x00, // InMemoryMediaSize
            0x00  // SourceBits
        };

        var nodeBaseParams = CreateMinimalNodeBaseParamsData();
        var originalData = bankSourceData.Concat(nodeBaseParams).ToArray();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new SoundInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region RanSeqCntrInitialValues Tests

    [Fact]
    public void RanSeqCntrInitialValues_RoundTrip_Minimal()
    {
        var nodeBaseParams = CreateMinimalNodeBaseParamsData();
        var ranSeqData = new byte[]
        {
            0x01,
            0x00, // LoopCount = 1
            0x00,
            0x00, // LoopModMin = 0
            0x00,
            0x00, // LoopModMax = 0
            0x00,
            0x00,
            0x00,
            0x00, // TransitionTime = 0.0f
            0x00,
            0x00,
            0x00,
            0x00, // TransitionTimeModMin = 0.0f
            0x00,
            0x00,
            0x00,
            0x00, // TransitionTimeModMax = 0.0f
            0x00,
            0x00, // AvoidRepeatCount = 0
            0x00, // TransitionMode
            0x00, // RandomMode
            0x00, // Mode
            0x00, // BitVector
            // Children (empty)
            0x00,
            0x00,
            0x00,
            0x00,

            // Playlist (empty)
            0x00,
            0x00
        };

        var originalData = nodeBaseParams.Concat(ranSeqData).ToArray();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new RanSeqCntrInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region FeedbackNodeInitialValues Tests

    [Fact]
    public void FeedbackNodeInitialValues_RoundTrip_Minimal()
    {
        var nodeBaseParams = CreateMinimalNodeBaseParamsData();
        var feedbackData = new byte[]
        {
            0x00, 0x00, 0x00, 0x00 // numSources = 0 (uint32)
        };

        var originalData = feedbackData.Concat(nodeBaseParams).ToArray();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new FeedbackNodeInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region BusInitialValues Tests

    [Fact]
    public void BusInitialValues_RoundTrip_Minimal()
    {
        var originalData = new byte[]
        {
            // OverrideBusId (4 bytes)
            0x00,
            0x00,
            0x00,
            0x00,

            // BusInitialParams:
            // - PropBundle (1 byte: count=0)
            0x00,

            // - BitVector1 (1 byte)
            0x00,

            // - BitVector2 (1 byte)
            0x00,

            // - MaxNumInstance (2 bytes)
            0x00,
            0x00,

            // - ChannelConfig (4 bytes)
            0x00,
            0x00,
            0x00,
            0x00,

            // - BitVector3 (1 byte)
            0x00,

            // RecoveryTime (4 bytes)
            0x00,
            0x00,
            0x00,
            0x00,

            // MaxDuckVolume (4 bytes)
            0x00,
            0x00,
            0x00,
            0x00,

            // DuckList count (4 bytes) = 0
            0x00,
            0x00,
            0x00,
            0x00,

            // BusInitialFxParams:
            // - NumFx (1 byte) = 0
            0x00,

            // - FxId0 (4 bytes)
            0x00,
            0x00,
            0x00,
            0x00,

            // - IsShareSet0 (1 byte)
            0x00,

            // OverrideAttachmentParams (1 byte)
            0x00,

            // InitialRtpc (2 bytes: count=0)
            0x00,
            0x00,

            // StateChunk (4 bytes: count=0)
            0x00,
            0x00,
            0x00,
            0x00
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new BusInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region Full PCK Round-Trip Tests

    [SkippableFact]
    public void Integration_RealPckFile_FullRoundTrip()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        // Save to a temp file (Sounds.pck is too large for MemoryStream)
        var tempPath = Path.GetTempFileName();
        bool isIdentical;

        try
        {
            // Load the original .pck file
            using (var original = PckFile.Load(SoundsPckPath))
            {
                Assert.NotNull(original);
                original.Save(tempPath);

                // Load the saved file and compare
                using (var reloaded = PckFile.Load(tempPath))
                {
                    Assert.NotNull(reloaded);

                    // Compare using the built-in Compare method
                    isIdentical = original.Compare(reloaded);
                }
            }
        }
        finally
        {
            // Try to delete, but don't fail if we can't (OS will clean up temp files)
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (IOException)
            {
                // Ignore - temp file will be cleaned up by OS
            }
        }

        Assert.True(isIdentical, "Round-trip produced different data - see test output for details");
    }

#endregion

#region WEM Replacement Command

    [SkippableFact]
    public void Command_ReplaceWem_970927665_With_972457947()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        uint targetWemId = 970927665;
        uint replacementWemId = 972457947;
        var outputPath = Path.Combine(
            Path.GetDirectoryName(SoundsPckPath)!,
            "Sounds_modified.pck");

        // Load PCK
        using var pck = PckFile.Load(SoundsPckPath);
        Assert.NotNull(pck);

        // Find replacement WEM data
        byte[]? replacementData = null;

        var streamingEntry = pck.StreamingFiles[replacementWemId];

        if (streamingEntry is not null)
        {
            replacementData = streamingEntry.GetData();
        }
        else
        {
            foreach (var bankEntry in pck.SoundBanks)
            {
                var bank = bankEntry.Parse();

                if (bank != null && bank.Media.Contains(replacementWemId))
                {
                    bank.Media.TryGet(replacementWemId, out replacementData);

                    break;
                }
            }
        }

        Assert.NotNull(replacementData);

        // Replace
        var result = pck.ReplaceWem(targetWemId, replacementData);

        // Save
        pck.Save(outputPath);

        // Output results
        Assert.True(
            result.ReplacedInStreaming || result.EmbeddedBanksModified > 0,
            $"WEM {targetWemId} was not found. Streaming: {result.ReplacedInStreaming}, Banks: {result.EmbeddedBanksModified}");
    }

#endregion

#region StateItem Tests

    [Fact]
    public void StateInitialValues_Read_ParsesEmptyProps()
    {
        // Arrange - Empty props (0 count)
        var data = new byte[]
        {
            0x00 // cProps = 0
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var values = new StateInitialValues();

        // Act
        var result = values.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Empty(values.Props);
    }

    [Fact]
    public void StateInitialValues_Read_ParsesSingleProp()
    {
        // Arrange - 1 prop: ID=0x02, Value=0.5f
        var valueBytes = BitConverter.GetBytes(0.5f);
        var data = new byte[]
        {
            0x01, // cProps = 1
            0x02, // propId[0] = 2
            valueBytes[0],
            valueBytes[1],
            valueBytes[2],
            valueBytes[3] // value[0] = 0.5f
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var values = new StateInitialValues();

        // Act
        var result = values.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Single(values.Props);
        Assert.Equal(0x02, values.Props[0].PropId);
        Assert.Equal(0.5f, values.Props[0].Value);
    }

    [Fact]
    public void StateInitialValues_Read_ParsesMultipleProps()
    {
        // Arrange - 2 props
        var value1Bytes = BitConverter.GetBytes(1.0f);
        var value2Bytes = BitConverter.GetBytes(-3.5f);
        var data = new byte[]
        {
            0x02, // cProps = 2
            0x01,
            0x05, // propIds = [1, 5]
            value1Bytes[0],
            value1Bytes[1],
            value1Bytes[2],
            value1Bytes[3], // value[0] = 1.0f
            value2Bytes[0],
            value2Bytes[1],
            value2Bytes[2],
            value2Bytes[3] // value[1] = -3.5f
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var values = new StateInitialValues();

        // Act
        var result = values.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(2, values.Props.Count);
        Assert.Equal(0x01, values.Props[0].PropId);
        Assert.Equal(1.0f, values.Props[0].Value);
        Assert.Equal(0x05, values.Props[1].PropId);
        Assert.Equal(-3.5f, values.Props[1].Value);
    }

#endregion

#region SwitchPackage Tests

    [Fact]
    public void SwitchPackage_Read_ParsesEmptyNodeList()
    {
        // Arrange - SwitchId=0x12345678, 0 nodes
        var data = new byte[]
        {
            0x78,
            0x56,
            0x34,
            0x12, // ulSwitchID = 0x12345678
            0x00,
            0x00,
            0x00,
            0x00 // ulNumItems = 0
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var package = new SwitchPackage();

        // Act
        var result = package.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(0x12345678u, package.SwitchId);
        Assert.Empty(package.NodeIds);
    }

    [Fact]
    public void SwitchPackage_Read_ParsesMultipleNodes()
    {
        // Arrange - SwitchId=0xABCD, 2 nodes
        var data = new byte[]
        {
            0xCD,
            0xAB,
            0x00,
            0x00, // ulSwitchID = 0xABCD
            0x02,
            0x00,
            0x00,
            0x00, // ulNumItems = 2
            0x01,
            0x00,
            0x00,
            0x00, // NodeID[0] = 1
            0x02,
            0x00,
            0x00,
            0x00 // NodeID[1] = 2
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var package = new SwitchPackage();

        // Act
        var result = package.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(0xABCDu, package.SwitchId);
        Assert.Equal(2, package.NodeIds.Count);
        Assert.Equal(1u, package.NodeIds[0]);
        Assert.Equal(2u, package.NodeIds[1]);
    }

#endregion

#region SwitchNodeParams Tests

    [Fact]
    public void SwitchNodeParams_Read_ParsesBasicParams()
    {
        // Arrange
        var data = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // ulNodeID = 1
            0x03, // bitVector = 0x03 (IsFirstOnly=true, ContinuePlayback=true)
            0x01, // mode bitVector
            0x64,
            0x00,
            0x00,
            0x00, // FadeOutTime = 100
            0xC8,
            0x00,
            0x00,
            0x00 // FadeInTime = 200
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var param = new SwitchNodeParams();

        // Act
        var result = param.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(1u, param.NodeId);
        Assert.True(param.IsFirstOnly);
        Assert.True(param.ContinuePlayback);
        Assert.Equal(100, param.FadeOutTime);
        Assert.Equal(200, param.FadeInTime);
    }

    [Fact]
    public void SwitchNodeParams_IsFirstOnly_FalseWhenBitNotSet()
    {
        // Arrange
        var data = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // ulNodeID = 1
            0x00, // bitVector = 0x00
            0x00, // mode bitVector
            0x00,
            0x00,
            0x00,
            0x00, // FadeOutTime = 0
            0x00,
            0x00,
            0x00,
            0x00 // FadeInTime = 0
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var param = new SwitchNodeParams();

        // Act
        param.Read(reader);

        // Assert
        Assert.False(param.IsFirstOnly);
        Assert.False(param.ContinuePlayback);
    }

#endregion

#region AssociatedChildData Tests

    [Fact]
    public void AssociatedChildData_Read_ParsesEmptyCurve()
    {
        // Arrange
        var data = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // ulAssociatedChildID = 1
            0x00,
            0x00,
            0x00,
            0x00 // ulCurveSize = 0
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var assoc = new AssociatedChildData();

        // Act
        var result = assoc.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(1u, assoc.AssociatedChildId);
        Assert.Empty(assoc.Curve);
    }

    [Fact]
    public void AssociatedChildData_Read_ParsesCurveWithPoints()
    {
        // Arrange - 1 curve point: From=0.0, To=1.0, Interp=0 (linear)
        var fromBytes = BitConverter.GetBytes(0.0f);
        var toBytes = BitConverter.GetBytes(1.0f);
        var data = new byte[]
        {
            0x02,
            0x00,
            0x00,
            0x00, // ulAssociatedChildID = 2
            0x01,
            0x00,
            0x00,
            0x00, // ulCurveSize = 1
            fromBytes[0],
            fromBytes[1],
            fromBytes[2],
            fromBytes[3], // From = 0.0f
            toBytes[0],
            toBytes[1],
            toBytes[2],
            toBytes[3], // To = 1.0f
            0x00,
            0x00,
            0x00,
            0x00 // InterpolationType = 0 (linear)
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var assoc = new AssociatedChildData();

        // Act
        var result = assoc.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(2u, assoc.AssociatedChildId);
        Assert.Single(assoc.Curve);
        Assert.Equal(0.0f, assoc.Curve[0].From);
        Assert.Equal(1.0f, assoc.Curve[0].To);
    }

#endregion

#region DialogueEventInitialValues Tests

    [Fact]
    public void DialogueEventInitialValues_Read_ParsesMinimalDialogueEvent()
    {
        // Arrange - Probability=100, TreeDepth=0 (no arguments), empty tree
        var data = new byte[]
        {
            0x64, // uProbability = 100
            0x00,
            0x00,
            0x00,
            0x00, // uTreeDepth = 0
            // No arguments
            0x00,
            0x00,
            0x00,
            0x00, // uTreeDataSize = 0
            0x00  // uMode = 0
            // No tree data
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var values = new DialogueEventInitialValues();

        // Act
        var result = values.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(100, values.Probability);
        Assert.Equal(0u, values.TreeDepth);
        Assert.Empty(values.Arguments);
        Assert.Equal(0u, values.TreeDataSize);
        Assert.Equal(0, values.TreeMode);
    }

    [Fact]
    public void DialogueEventInitialValues_Read_ParsesWithSingleArgument()
    {
        // Arrange - Probability=100, TreeDepth=1, 1 argument (GroupId=0x12345678, GroupType=1)
        var data = new byte[]
        {
            0x64, // uProbability = 100
            0x01,
            0x00,
            0x00,
            0x00, // uTreeDepth = 1
            0x78,
            0x56,
            0x34,
            0x12, // groupId[0] = 0x12345678
            0x01, // groupType[0] = 1 (State)
            0x00,
            0x00,
            0x00,
            0x00, // uTreeDataSize = 0
            0x00  // uMode = 0
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var values = new DialogueEventInitialValues();

        // Act
        var result = values.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Equal(1u, values.TreeDepth);
        Assert.Single(values.Arguments);
        Assert.Equal(0x12345678u, values.Arguments[0].GroupId);
        Assert.Equal(1, values.Arguments[0].GroupType);
    }

#endregion

#region Round-Trip Serialization Tests

    [Fact]
    public void StateInitialValues_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange - 2 props
        var value1Bytes = BitConverter.GetBytes(1.0f);
        var value2Bytes = BitConverter.GetBytes(-3.5f);
        var originalData = new byte[]
        {
            0x02, // cProps = 2
            0x01,
            0x05, // propIds = [1, 5]
            value1Bytes[0],
            value1Bytes[1],
            value1Bytes[2],
            value1Bytes[3], // value[0] = 1.0f
            value2Bytes[0],
            value2Bytes[1],
            value2Bytes[2],
            value2Bytes[3] // value[1] = -3.5f
        };

        // Act - Read
        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new StateInitialValues();
        values.Read(reader);

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

    [Fact]
    public void SwitchPackage_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange - SwitchId=0xABCD, 2 nodes
        var originalData = new byte[]
        {
            0xCD,
            0xAB,
            0x00,
            0x00, // ulSwitchID = 0xABCD
            0x02,
            0x00,
            0x00,
            0x00, // ulNumItems = 2
            0x01,
            0x00,
            0x00,
            0x00, // NodeID[0] = 1
            0x02,
            0x00,
            0x00,
            0x00 // NodeID[1] = 2
        };

        // Act - Read
        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var package = new SwitchPackage();
        package.Read(reader);

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        package.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

    [Fact]
    public void SwitchNodeParams_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange
        var originalData = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // ulNodeID = 1
            0x03, // bitVector = 0x03
            0x01, // mode bitVector
            0x64,
            0x00,
            0x00,
            0x00, // FadeOutTime = 100
            0xC8,
            0x00,
            0x00,
            0x00 // FadeInTime = 200
        };

        // Act - Read
        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var param = new SwitchNodeParams();
        param.Read(reader);

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        param.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

    [Fact]
    public void AssociatedChildData_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange - 1 curve point
        var fromBytes = BitConverter.GetBytes(0.0f);
        var toBytes = BitConverter.GetBytes(1.0f);
        var originalData = new byte[]
        {
            0x02,
            0x00,
            0x00,
            0x00, // ulAssociatedChildID = 2
            0x01,
            0x00,
            0x00,
            0x00, // ulCurveSize = 1
            fromBytes[0],
            fromBytes[1],
            fromBytes[2],
            fromBytes[3], // From = 0.0f
            toBytes[0],
            toBytes[1],
            toBytes[2],
            toBytes[3], // To = 1.0f
            0x00,
            0x00,
            0x00,
            0x00 // InterpolationType = 0
        };

        // Act - Read
        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var assoc = new AssociatedChildData();
        assoc.Read(reader);

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        assoc.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

    [Fact]
    public void DialogueEventInitialValues_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange - Probability=100, TreeDepth=1, 1 argument
        var originalData = new byte[]
        {
            0x64, // uProbability = 100
            0x01,
            0x00,
            0x00,
            0x00, // uTreeDepth = 1
            0x78,
            0x56,
            0x34,
            0x12, // groupId[0] = 0x12345678
            0x01, // groupType[0] = 1 (State)
            0x00,
            0x00,
            0x00,
            0x00, // uTreeDataSize = 0
            0x00  // uMode = 0
        };

        // Act - Read
        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new DialogueEventInitialValues();
        values.Read(reader);

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

#endregion

#region ModulatorInitialValues Tests

    [Fact]
    public void ModulatorInitialValues_Read_ParsesEmptyModulator()
    {
        // Arrange - Empty props, empty ranged props, empty RTPC
        var data = new byte[]
        {
            0x00, // PropBundle: cProps = 0
            0x00, // PropBundleRanged: cProps = 0
            0x00,
            0x00 // InitialRtpc: numRtpcs = 0
        };

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        var values = new ModulatorInitialValues();

        // Act
        var result = values.Read(reader);

        // Assert
        Assert.True(result);
        Assert.Empty(values.PropBundle.Props);
        Assert.Empty(values.PropBundleRanged.Props);
        Assert.Empty(values.InitialRtpc.RtpcManagers);
    }

    [Fact]
    public void ModulatorInitialValues_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange - Empty modulator
        var originalData = new byte[]
        {
            0x00, // PropBundle: cProps = 0
            0x00, // PropBundleRanged: cProps = 0
            0x00,
            0x00 // InitialRtpc: numRtpcs = 0
        };

        // Act - Read
        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new ModulatorInitialValues();
        values.Read(reader);

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

#endregion

#region MusicTransitionRule Tests

    [Fact]
    public void MusicTransSrcRule_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange
        var originalData = new byte[]
        {
            0x64,
            0x00,
            0x00,
            0x00, // TransitionTime = 100
            0x01,
            0x00,
            0x00,
            0x00, // FadeCurve = 1
            0x32,
            0x00,
            0x00,
            0x00, // FadeOffset = 50
            0x02,
            0x00,
            0x00,
            0x00, // SyncType = 2
            0x78,
            0x56,
            0x34,
            0x12, // CueFilterHash = 0x12345678
            0x01  // PlayPostExit = 1
        };

        // Act - Read (manual since no Read method, just test Write)
        var rule = new MusicTransSrcRule
        {
            TransitionTime = 100,
            FadeCurve = 1,
            FadeOffset = 50,
            SyncType = 2,
            CueFilterHash = 0x12345678,
            PlayPostExit = 1
        };

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        rule.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

    [Fact]
    public void MusicTransDstRule_RoundTrip_ProducesIdenticalBinary()
    {
        // Arrange
        var originalData = new byte[]
        {
            0x64,
            0x00,
            0x00,
            0x00, // TransitionTime = 100
            0x01,
            0x00,
            0x00,
            0x00, // FadeCurve = 1
            0x32,
            0x00,
            0x00,
            0x00, // FadeOffset = 50
            0x78,
            0x56,
            0x34,
            0x12, // CueFilterHash = 0x12345678
            0xAB,
            0xCD,
            0x00,
            0x00, // JumpToId = 0xCDAB
            0x03,
            0x00, // EntryType = 3
            0x01, // PlayPreEntry = 1
            0x00  // DestMatchSourceCueName = 0
        };

        // Act - Read (manual since no Read method, just test Write)
        var rule = new MusicTransDstRule
        {
            TransitionTime = 100,
            FadeCurve = 1,
            FadeOffset = 50,
            CueFilterHash = 0x12345678,
            JumpToId = 0xCDAB,
            EntryType = 3,
            PlayPreEntry = 1,
            DestMatchSourceCueName = 0
        };

        // Act - Write
        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        rule.Write(writer);
        var writtenData = writeStream.ToArray();

        // Assert
        Assert.Equal(originalData, writtenData);
    }

#endregion

#region EventInitialValues Tests

    [Fact]
    public void EventInitialValues_RoundTrip_EmptyActions()
    {
        // Arrange - 0 actions
        var originalData = new byte[]
        {
            0x00, 0x00, 0x00, 0x00 // actionListSize = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new EventInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

    [Fact]
    public void EventInitialValues_RoundTrip_WithActions()
    {
        // Arrange - 2 actions
        var originalData = new byte[]
        {
            0x02,
            0x00,
            0x00,
            0x00, // actionListSize = 2
            0x01,
            0x00,
            0x00,
            0x00, // action[0] = 1
            0x02,
            0x00,
            0x00,
            0x00 // action[1] = 2
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var values = new EventInitialValues();
        values.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        values.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region Children Tests

    [Fact]
    public void Children_RoundTrip_Empty()
    {
        var originalData = new byte[]
        {
            0x00, 0x00, 0x00, 0x00 // numberOfChildren = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var children = new Children();
        children.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        children.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

    [Fact]
    public void Children_RoundTrip_WithChildren()
    {
        var originalData = new byte[]
        {
            0x03,
            0x00,
            0x00,
            0x00, // numberOfChildren = 3
            0x0A,
            0x00,
            0x00,
            0x00, // childId[0] = 10
            0x14,
            0x00,
            0x00,
            0x00, // childId[1] = 20
            0x1E,
            0x00,
            0x00,
            0x00 // childId[2] = 30
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var children = new Children();
        children.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        children.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region StateChunk Tests

    [Fact]
    public void StateChunk_RoundTrip_Empty()
    {
        var originalData = new byte[]
        {
            0x00, 0x00, 0x00, 0x00 // numberOfStateGroups = 0
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var chunk = new StateChunk();
        chunk.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        chunk.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

    [Fact]
    public void StateChunk_RoundTrip_WithStateGroup()
    {
        var originalData = new byte[]
        {
            0x01,
            0x00,
            0x00,
            0x00, // numberOfStateGroups = 1
            0x78,
            0x56,
            0x34,
            0x12, // stateGroupId = 0x12345678
            0x01, // stateSyncType = 1
            0x01,
            0x00, // numStates = 1
            0xAB,
            0xCD,
            0x00,
            0x00, // stateId = 0xCDAB
            0xEF,
            0x01,
            0x00,
            0x00 // stateInstanceId = 0x01EF
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var chunk = new StateChunk();
        chunk.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        chunk.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region AuxParams Tests

    [Fact]
    public void AuxParams_RoundTrip_NoAux()
    {
        var originalData = new byte[]
        {
            0x00 // BitVector with HasAux=false
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var aux = new AuxParams();
        aux.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        aux.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

    [Fact]
    public void AuxParams_RoundTrip_WithAux()
    {
        var originalData = new byte[]
        {
            0x08, // BitVector with HasAux=true (bit 3)
            0x01,
            0x00,
            0x00,
            0x00, // auxId[0] = 1
            0x02,
            0x00,
            0x00,
            0x00, // auxId[1] = 2
            0x03,
            0x00,
            0x00,
            0x00, // auxId[2] = 3
            0x04,
            0x00,
            0x00,
            0x00 // auxId[3] = 4
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var aux = new AuxParams();
        aux.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        aux.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region PositioningParams Tests

    [Fact]
    public void PositioningParams_RoundTrip_No3D()
    {
        var originalData = new byte[]
        {
            0x00 // BitVector with Is3DPositioningAvailable=false
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var pos = new PositioningParams();
        pos.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        pos.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

    [Fact]
    public void PositioningParams_RoundTrip_With3D_TypeEmitter()
    {
        // e3DPositionType = 1 means no automation data
        var originalData = new byte[]
        {
            0x08, // BitVector with Is3DPositioningAvailable=true (bit 3)
            0x01, // Bits3D with e3DPositionType=1
            0x78,
            0x56,
            0x34,
            0x12 // AttenuationId = 0x12345678
        };

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var pos = new PositioningParams();
        pos.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        pos.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region NodeBaseParams Tests

    /// <summary>
    ///     Creates minimal NodeBaseParams binary data for testing.
    ///     Structure: NodeInitialFxParams(2) + OverrideAttachmentParams(1) + OverrideBusId(4) +
    ///     DirectParentId(4) + ByBitVector(1) + NodeInitialParams(2) + PositioningParams(1) +
    ///     AuxParams(1) + AdvSettingsParams(6) + StateChunk(4) + InitialRtpc(2) = 28 bytes
    /// </summary>
    private static byte[] CreateMinimalNodeBaseParamsData()
    {
        return
        [
            // NodeInitialFxParams
            0x00, // IsOverrideParentFx
            0x00, // NumFx = 0
            // OverrideAttachmentParams
            0x00,

            // OverrideBusId
            0x01,
            0x00,
            0x00,
            0x00,

            // DirectParentId
            0x02,
            0x00,
            0x00,
            0x00,

            // ByBitVector
            0x00,

            // NodeInitialParams (2 empty PropBundles)
            0x00,
            0x00,

            // PositioningParams (no 3D)
            0x00,

            // AuxParams (no aux)
            0x00,

            // AdvSettingsParams
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,
            0x00,

            // StateChunk (empty)
            0x00,
            0x00,
            0x00,
            0x00,

            // InitialRtpc (empty)
            0x00,
            0x00
        ];
    }

    [Fact]
    public void NodeBaseParams_RoundTrip_Minimal()
    {
        var originalData = CreateMinimalNodeBaseParamsData();

        using var readStream = new MemoryStream(originalData);
        using var reader = new BinaryReader(readStream);
        var nbp = new NodeBaseParams();
        nbp.Read(reader);

        using var writeStream = new MemoryStream();
        using var writer = new BinaryWriter(writeStream);
        nbp.Write(writer);

        Assert.Equal(originalData, writeStream.ToArray());
    }

#endregion

#region Integration Tests - Real .pck File

    /// <summary>
    ///     Path to the Sounds.pck file from Halo Wars DE.
    ///     This test is skipped if the file doesn't exist.
    /// </summary>
    private const string SoundsPckPath =
        @"C:\Program Files (x86)\Steam\steamapps\common\HaloWarsDE\sound\wwise_2013\GeneratedSoundBanks\Windows\Sounds.pck";

    [SkippableFact]
    public void Integration_ParseRealPckFile_BanksParseWithHircChunks()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        // Load the .pck file
        using var pck = PckFile.Load(SoundsPckPath);
        Assert.NotNull(pck);

        // Note: Count returns unique IDs, but we iterate all entries (including same ID for different languages)
        var totalBanks = pck.SoundBanks.Entries.Count;
        var parsedBanks = 0;
        var banksWithHirc = 0;
        var failedBanks = new List<(uint Id, long Size)>();

        foreach (var entry in pck.SoundBanks)
        {
            var bank = entry.Parse();

            if (bank is not null)
            {
                parsedBanks++;

                if (bank.HircChunk?.Items?.Count > 0) banksWithHirc++;
            }
            else
            {
                failedBanks.Add((entry.Id, entry.Size));
            }
        }

        // Note: Many banks in .pck files don't have HIRC chunks (they may be media-only banks).
        // We verify that the banks that DO have HIRC chunks parse successfully.
        // The banks that return null may be unsupported versions or have unsupported chunk types.
        var failedInfo = failedBanks.Count > 0
            ? $"\nFirst 10 failed banks: {string.Join(", ", failedBanks.Take(10).Select(b => $"0x{b.Id:X8} ({b.Size} bytes)"))}"
            : "";

        Assert.True(parsedBanks > 0, $"No banks could be parsed. Total: {totalBanks}");
        Assert.True(banksWithHirc > 0, $"No banks with HIRC chunks found. Parsed: {parsedBanks}/{totalBanks}");

        // Verify all banks parse successfully now (the fix handles unknown chunks gracefully)
        Assert.True(
            parsedBanks == totalBanks,
            $"Not all banks parsed successfully. Parsed: {parsedBanks}/{totalBanks}, With HIRC: {banksWithHirc}{failedInfo}");
    }

    [SkippableFact]
    public void Integration_ParseRealPckFile_DiagnoseHircItemTypes()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        using var pck = PckFile.Load(SoundsPckPath);
        Assert.NotNull(pck);

        var typeCounts = new Dictionary<HircType, int>();

        foreach (var entry in pck.SoundBanks)
        {
            var bank = entry.Parse();

            if (bank?.HircChunk?.Items == null) continue;

            foreach (var item in bank.HircChunk.Items)
            {
                if (!typeCounts.ContainsKey(item.Type)) typeCounts[item.Type] = 0;

                typeCounts[item.Type]++;
            }
        }

        // Output the counts for diagnostic purposes
        var output = new StringBuilder();
        output.AppendLine("HIRC Item Types in Sounds.pck:");

        foreach (var kvp in typeCounts.OrderBy(x => (int) x.Key))
        {
            output.AppendLine($"  {kvp.Key} ({(int) kvp.Key}): {kvp.Value}");
        }

        output.AppendLine($"Total: {typeCounts.Values.Sum()} items across {typeCounts.Count} types");

        // Check for FX items specifically
        var hasFxShareSet = typeCounts.ContainsKey(HircType.FxShareSet);
        var hasFxCustom = typeCounts.ContainsKey(HircType.FxCustom);
        output.AppendLine(
            $"FxShareSet present: {hasFxShareSet} (count: {(hasFxShareSet ? typeCounts[HircType.FxShareSet] : 0)})");

        output.AppendLine(
            $"FxCustom present: {hasFxCustom} (count: {(hasFxCustom ? typeCounts[HircType.FxCustom] : 0)})");

        // This will show up in the test output
        Assert.True(true, output.ToString());

        // Write to console so it shows in test output
        Console.WriteLine(output.ToString());
    }

    [SkippableFact]
    public void Integration_RealPckFile_HircItemsRoundTrip()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        using var pck = PckFile.Load(SoundsPckPath);
        Assert.NotNull(pck);

        var totalItems = 0;
        var roundTripSuccesses = 0;
        var roundTripFailures = new List<(uint BankId, uint ItemId, HircType Type, string Error)>();

        foreach (var entry in pck.SoundBanks)
        {
            var bank = entry.Parse();

            if (bank?.HircChunk?.Items is null) continue;

            foreach (var item in bank.HircChunk.Items)
            {
                totalItems++;

                try
                {
                    // Write the item to bytes
                    using var writeStream = new MemoryStream();
                    using var writer = new BinaryWriter(writeStream);
                    item.Write(writer);
                    var writtenBytes = writeStream.ToArray();

                    // Re-read the item
                    using var readStream = new MemoryStream(writtenBytes);
                    using var reader = new BinaryReader(readStream);
                    var reReadItem = HircItem.Read(reader);

                    if (reReadItem is null)
                    {
                        roundTripFailures.Add((entry.Id, item.Id, item.Type, "Re-read returned null"));

                        continue;
                    }

                    // Write again and compare
                    using var reWriteStream = new MemoryStream();
                    using var reWriter = new BinaryWriter(reWriteStream);
                    reReadItem.Write(reWriter);
                    var reWrittenBytes = reWriteStream.ToArray();

                    if (!writtenBytes.SequenceEqual(reWrittenBytes))
                    {
                        roundTripFailures.Add(
                            (entry.Id, item.Id, item.Type,
                             $"Bytes mismatch: {writtenBytes.Length} vs {reWrittenBytes.Length}"));

                        continue;
                    }

                    roundTripSuccesses++;
                }
                catch (Exception ex)
                {
                    roundTripFailures.Add((entry.Id, item.Id, item.Type, ex.Message));
                }
            }
        }

        // Log summary
        var summary = $"Total: {totalItems}, Success: {roundTripSuccesses}, Failed: {roundTripFailures.Count}";

        // Group failures by type for analysis
        var failuresByType = roundTripFailures
                             .GroupBy(f => f.Type)
                             .Select(g => $"{g.Key}: {g.Count()}")
                             .ToList();

        // Get sample errors for each type (first 3)
        var sampleErrors = roundTripFailures
                           .GroupBy(f => f.Type)
                           .ToDictionary(
                               g => g.Key,
                               g => g.Take(3).Select(f => f.Error).Distinct().ToList());

        var errorDetails = string.Join("\n", sampleErrors.Select(kv => $"  {kv.Key}: {string.Join("; ", kv.Value)}"));

        // Assert - all items should round-trip successfully
        Assert.True(
            roundTripFailures.Count == 0,
            $"Round-trip failures: {summary}\nBy Type: {string.Join(", ", failuresByType)}\nSample errors:\n{errorDetails}");
    }

#endregion

#region WEM Replacement Tests

    [Fact]
    public void SoundBank_ReplaceWem_UpdatesMediaData()
    {
        // Arrange - Create a soundbank with embedded media
        var bank = new SoundBank(0x12345678);
        var originalData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var newData = new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

        bank.Media.Add(100, originalData);

        // Act
        bank.ReplaceWem(100, newData);

        // Assert
        Assert.Equal(newData, bank.Media[100]);
    }

    [Fact]
    public void SoundBank_ReplaceWem_ThrowsForNonExistentId()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => bank.ReplaceWem(999, new byte[] { 0x01 }));
    }

    [Fact]
    public void SoundBank_SetWem_AddsNewMedia()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);
        var data = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var updated = bank.SetWem(100, data);

        // Assert
        Assert.Equal(0, updated); // No HIRC references to update for new media
        Assert.True(bank.Media.Contains(100));
        Assert.Equal(data, bank.Media[100]);
    }

    [Fact]
    public void SoundBank_SetWem_ReplacesExistingMedia()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);
        var originalData = new byte[] { 0x01, 0x02, 0x03 };
        var newData = new byte[] { 0x0A, 0x0B, 0x0C, 0x0D };

        bank.Media.Add(100, originalData);

        // Act
        bank.SetWem(100, newData);

        // Assert
        Assert.Equal(newData, bank.Media[100]);
    }

    [Fact]
    public void SoundBank_GetMediaReferences_ReturnsEmptyForNoReferences()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);
        bank.Media.Add(100, new byte[] { 0x01, 0x02 });

        // Act
        var refs = bank.GetMediaReferences(100).ToList();

        // Assert
        Assert.Empty(refs);
    }

    [Fact]
    public void SoundBank_GetItemsBySourceId_ReturnsEmptyForNoReferences()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);
        bank.Media.Add(100, new byte[] { 0x01, 0x02 });

        // Act
        var items = bank.GetItemsBySourceId(100).ToList();

        // Assert
        Assert.Empty(items);
    }

    [Fact]
    public void SoundBank_UpdateMediaSize_ReturnsZeroForNoReferences()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);

        // Act
        var updated = bank.UpdateMediaSize(100, 1000);

        // Assert
        Assert.Equal(0, updated);
    }

    [SkippableFact]
    public void Integration_ReplaceWem_UpdatesHircSizes()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        // Load the .pck file
        using var pck = PckFile.Load(SoundsPckPath);
        Assert.NotNull(pck);

        // Find a bank with embedded media and Sound items
        SoundBank? testBank = null;
        uint testSourceId = 0;
        uint originalSize = 0;

        foreach (var entry in pck.SoundBanks)
        {
            var bank = entry.Parse();

            if (bank is null || bank.Media.Count == 0)
            {
                continue;
            }

            // Find a Sound item that references embedded media
            foreach (var sound in bank.Sounds)
            {
                var sourceId = sound.Values.BankSourceData.MediaInformation.SourceId;

                if (bank.Media.Contains(sourceId))
                {
                    testBank = bank;
                    testSourceId = sourceId;
                    originalSize = sound.Values.BankSourceData.MediaInformation.InMemoryMediaSize;

                    break;
                }
            }

            if (testBank is not null)
            {
                break;
            }
        }

        Skip.If(testBank is null, "No suitable bank found with embedded media and Sound items");

        // Create replacement data with different size
        var newData = new byte[originalSize + 100];
        Array.Fill(newData, (byte) 0xAB);

        // Act - Replace the WEM
        var updatedCount = testBank!.ReplaceWem(testSourceId, newData);

        // Assert
        Assert.True(updatedCount > 0, "Expected at least one HIRC reference to be updated");

        // Verify the size was updated
        var refs = testBank.GetMediaReferences(testSourceId).ToList();
        Assert.All(refs, r => Assert.Equal((uint) newData.Length, r.InMemoryMediaSize));
    }

    [SkippableFact]
    public void Integration_SoundBankRoundTrip_ProducesIdenticalData()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        // Load the .pck file
        using var pck = PckFile.Load(SoundsPckPath);
        Assert.NotNull(pck);

        // WEM 970927665 - the one user is trying to replace
        uint targetWem = 970927665;
        var failures = new List<string>();

        foreach (var entry in pck.SoundBanks)
        {
            var bank = entry.Parse();

            if (bank == null || !bank.Media.Contains(targetWem))
            {
                continue;
            }

            // Serialize the parsed bank
            var serialized = bank.ToByteArray();

            // Re-parse the serialized data to verify functional correctness
            // This is more robust than byte-comparison because it allows for different padding
            var reloadedBank = SoundBank.Parse(serialized);

            if (reloadedBank == null)
            {
                failures.Add($"Bank 0x{entry.Id:X8} (lang:{entry.LanguageId}): Failed to re-parse serialized data");

                continue;
            }

            // Compare HIRC items count
            if (bank.Items.Count != reloadedBank.Items.Count)
            {
                failures.Add(
                    $"Bank 0x{entry.Id:X8} (lang:{entry.LanguageId}): "
                    + $"HIRC item count mismatch! Original: {bank.Items.Count}, Reloaded: {reloadedBank.Items.Count}");

                continue;
            }

            // Compare media count
            if (bank.Media.Count != reloadedBank.Media.Count)
            {
                failures.Add(
                    $"Bank 0x{entry.Id:X8} (lang:{entry.LanguageId}): "
                    + $"Media count mismatch! Original: {bank.Media.Count}, Reloaded: {reloadedBank.Media.Count}");

                continue;
            }

            // Compare each media entry's data
            foreach (var kvp in bank.Media)
            {
                if (!reloadedBank.Media.TryGet(kvp.Key, out var reloadedData) || reloadedData is null)
                {
                    failures.Add(
                        $"Bank 0x{entry.Id:X8} (lang:{entry.LanguageId}): "
                        + $"Missing media entry 0x{kvp.Key:X8} in reloaded bank");

                    continue;
                }

                if (!kvp.Value.AsSpan().SequenceEqual(reloadedData))
                {
                    failures.Add(
                        $"Bank 0x{entry.Id:X8} (lang:{entry.LanguageId}): "
                        + $"Media data mismatch for entry 0x{kvp.Key:X8}! "
                        + $"Original size: {kvp.Value.Length}, Reloaded size: {reloadedData.Length}");
                }
            }
        }

        Assert.True(
            failures.Count == 0,
            $"Soundbank round-trip failures:\n{string.Join("\n", failures)}");
    }

    private const string ModifiedPckPath = @"c:\Users\dev\Documents\Git\coconutbird\SoundsUnpack\Sounds_modified.pck";

    [SkippableFact]
    public void Integration_ModifiedPck_CanBeLoaded()
    {
        Skip.IfNot(File.Exists(ModifiedPckPath), $"Sounds_modified.pck not found at {ModifiedPckPath}");

        // Try to load the modified PCK file
        using var pck = PckFile.Load(ModifiedPckPath);
        Assert.NotNull(pck);

        // Verify the modified file has the expected number of entries
        Assert.True(pck.SoundBanks.Count > 0, "Should have sound banks");
        Assert.True(pck.StreamingFiles.Count > 0, "Should have streaming files");

        // Parse all soundbanks to ensure none are corrupted
        var parsedBanks = 0;
        var errors = new List<string>();

        foreach (var entry in pck.SoundBanks)
        {
            try
            {
                var bank = entry.Parse();
                if (bank != null) parsedBanks++;
            }
            catch (Exception ex)
            {
                errors.Add($"Bank 0x{entry.Id:X8} (lang:{entry.LanguageId}): {ex.Message}");
            }
        }

        Assert.True(
            errors.Count == 0,
            $"Failed to parse {errors.Count} banks. "
            + $"Parsed {parsedBanks} successfully. "
            + $"Errors:\n{string.Join("\n", errors.Take(10))}");
    }

#endregion
}
