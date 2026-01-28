using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Bnk.Chunks;
using PckTool.Core.WWise.Common;

namespace PckTool.Core.Tests;

public class BankHeaderChunkTests
{
    private const uint ValidVersion = 0x71; // Wwise 2016.2

#region Magic Tests

    [Fact]
    public void Magic_ShouldBeBKHD()
    {
        var chunk = new BankHeaderChunk();

        Assert.Equal(Hash.AkmmioFourcc('B', 'K', 'H', 'D'), chunk.Magic);
    }

#endregion

#region IsValid Tests

    [Fact]
    public void IsValid_WithValidVersion_ShouldReturnTrue()
    {
        var chunk = new BankHeaderChunk
        {
            BankGeneratorVersion = ValidVersion,
            SoundBankId = 0x12345678,
            LanguageId = 0,
            FeedbackInBank = 0,
            ProjectId = 1000
        };

        Assert.True(chunk.IsValid);
    }

    [Fact]
    public void IsValid_WithInvalidVersion_ShouldReturnFalse()
    {
        var chunk = new BankHeaderChunk
        {
            BankGeneratorVersion = 0x72, // Wrong version
            SoundBankId = 0x12345678,
            LanguageId = 0,
            FeedbackInBank = 0,
            ProjectId = 1000
        };

        Assert.False(chunk.IsValid);
    }

    [Fact]
    public void IsValid_WithNullSoundBankId_ShouldReturnFalse()
    {
        var chunk = new BankHeaderChunk
        {
            BankGeneratorVersion = ValidVersion,
            SoundBankId = null, // Not set
            LanguageId = 0,
            FeedbackInBank = 0,
            ProjectId = 1000
        };

        Assert.False(chunk.IsValid);
    }

    [Fact]
    public void IsValid_WithNullProjectId_ShouldReturnFalse()
    {
        var chunk = new BankHeaderChunk
        {
            BankGeneratorVersion = ValidVersion,
            SoundBankId = 0x12345678,
            LanguageId = 0,
            FeedbackInBank = 0,
            ProjectId = null // Not set
        };

        Assert.False(chunk.IsValid);
    }

#endregion

#region Read Tests

    [Fact]
    public void Read_ValidHeader_ShouldParseCorrectly()
    {
        var chunk = new BankHeaderChunk();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Write valid header data
        writer.Write(ValidVersion); // BankGeneratorVersion
        writer.Write(0xABCDEF01u);  // SoundBankId
        writer.Write(0x00000002u);  // LanguageId
        writer.Write(0x00000000u);  // FeedbackInBank
        writer.Write(0x000003E8u);  // ProjectId (1000)

        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var soundBank = new SoundBank();

        var result = chunk.Read(soundBank, reader, 20);

        Assert.True(result);
        Assert.Equal(ValidVersion, chunk.BankGeneratorVersion);
        Assert.Equal(0xABCDEF01u, chunk.SoundBankId);
        Assert.Equal(0x00000002u, chunk.LanguageId);
        Assert.Equal(0x00000000u, chunk.FeedbackInBank);
        Assert.Equal(0x000003E8u, chunk.ProjectId);
    }

    [Fact]
    public void Read_WithPadding_ShouldHandlePadding()
    {
        var chunk = new BankHeaderChunk();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Write valid header data
        writer.Write(ValidVersion);
        writer.Write(0x12345678u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(1000u);

        // Add padding
        writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });

        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var soundBank = new SoundBank();

        var result = chunk.Read(soundBank, reader, 24); // 20 + 4 padding

        Assert.True(result);
        Assert.Equal(24, stream.Position); // Should read all including padding
    }

    [Fact]
    public void Read_InvalidVersion_ShouldReturnFalse()
    {
        var chunk = new BankHeaderChunk();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(0x70u); // Invalid version
        writer.Write(0x12345678u);
        writer.Write(0u);
        writer.Write(0u);
        writer.Write(1000u);

        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        var soundBank = new SoundBank();

        var result = chunk.Read(soundBank, reader, 20);

        Assert.False(result); // IsValid returns false due to wrong version
    }

#endregion
}
