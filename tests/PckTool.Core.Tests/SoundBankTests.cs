using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Tests;

public class SoundBankTests
{
#region Creation Tests

    [Fact]
    public void Create_WithId_ShouldSetProperties()
    {
        // Act
        var bank = new SoundBank(0x12345678, 1);

        // Assert
        Assert.Equal(0x12345678u, bank.Id);
        Assert.Equal(1u, bank.LanguageId);
        Assert.True(bank.IsValid);
    }

    [Fact]
    public void ToByteArray_ShouldSerializeBasicBank()
    {
        // Arrange
        var bank = new SoundBank(0x12345678) { Version = 0x71, ProjectId = 100, FeedbackInBank = 0 };

        // Act
        var bytes = bank.ToByteArray();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        // Verify BKHD magic at start (little endian: 'BKHD')
        Assert.Equal((byte) 'B', bytes[0]);
        Assert.Equal((byte) 'K', bytes[1]);
        Assert.Equal((byte) 'H', bytes[2]);
        Assert.Equal((byte) 'D', bytes[3]);
    }

    [Fact]
    public void Save_ShouldWriteToFile()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();

        try
        {
            var bank = new SoundBank(0x12345678) { Version = 0x71 };

            // Act
            bank.Save(tempPath);

            // Assert
            Assert.True(File.Exists(tempPath));
            var bytes = File.ReadAllBytes(tempPath);
            Assert.True(bytes.Length > 0);
            Assert.Equal((byte) 'B', bytes[0]);
            Assert.Equal((byte) 'K', bytes[1]);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void RoundTrip_BasicBank()
    {
        // Arrange
        var original = new SoundBank(0x12345678, 5) { Version = 0x71, ProjectId = 999, FeedbackInBank = 1 };

        // Act
        var bytes = original.ToByteArray();
        var loaded = SoundBank.Parse(bytes);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(original.Id, loaded.Id);
        Assert.Equal(original.LanguageId, loaded.LanguageId);
        Assert.Equal(original.Version, loaded.Version);
        Assert.Equal(original.ProjectId, loaded.ProjectId);
        Assert.Equal(original.FeedbackInBank, loaded.FeedbackInBank);
    }

#endregion

#region PckFile Integration Tests

    [Fact]
    public void PckFile_AddSoundBank_FromSoundBankObject_ShouldWork()
    {
        // Arrange
        var pck = PckFile.Create();
        var bank = new SoundBank(0x12345678, 1) { Version = 0x71, ProjectId = 42 };

        // Act
        var entry = pck.AddSoundBank(bank, name: "FromObject");

        // Assert
        Assert.Single(pck.SoundBanks);
        Assert.Equal(0x12345678u, entry.Id);
        Assert.Equal(1u, entry.LanguageId);
        Assert.Equal("FromObject", entry.Name);

        // Verify the data is valid BNK data
        var data = entry.GetData();
        Assert.Equal((byte) 'B', data[0]);
        Assert.Equal((byte) 'K', data[1]);
        Assert.Equal((byte) 'H', data[2]);
        Assert.Equal((byte) 'D', data[3]);
    }

#endregion

#region WEM Replacement Tests

    [Fact]
    public void ReplaceWem_UpdatesMediaData()
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
    public void ReplaceWem_ThrowsForNonExistentId()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => bank.ReplaceWem(999, new byte[] { 0x01 }));
    }

    [Fact]
    public void SetWem_AddsNewMedia()
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
    public void SetWem_ReplacesExistingMedia()
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
    public void GetMediaReferences_ReturnsEmptyForNoReferences()
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
    public void GetItemsBySourceId_ReturnsEmptyForNoReferences()
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
    public void UpdateMediaSize_ReturnsZeroForNoReferences()
    {
        // Arrange
        var bank = new SoundBank(0x12345678);

        // Act
        var updated = bank.UpdateMediaSize(100, 1000);

        // Assert
        Assert.Equal(0, updated);
    }

#endregion
}

