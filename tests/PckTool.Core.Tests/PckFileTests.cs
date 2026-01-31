using PckTool.Core.WWise.Pck;
using PckTool.Core.WWise.Pck.Entries;

namespace PckTool.Core.Tests;

public class PckFileTests
{
#region SoundBank Replacement Tests

    [Fact]
    public void ReplaceSoundBank_ById_ReturnsTrue_WhenEntryExists()
    {
        // Arrange
        var pck = CreateTestPckFile();
        var replacementData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        // Act
        var result = pck.ReplaceSoundBank(0x12345678, replacementData);

        // Assert
        Assert.True(result);
        Assert.True(pck.SoundBanks[0x12345678]!.IsModified);
        Assert.Equal(replacementData, pck.SoundBanks[0x12345678]!.GetData());
    }

    [Fact]
    public void ReplaceSoundBank_ById_ReturnsFalse_WhenEntryNotFound()
    {
        // Arrange
        var pck = CreateTestPckFile();

        // Act
        var result = pck.ReplaceSoundBank(0xDEADBEEF, [0x01, 0x02]);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ReplaceSoundBank_WithFilePath_ReturnsTrue_WhenEntryExists()
    {
        // Arrange
        var pck = CreateTestPckFile();
        var tempPath = Path.GetTempFileName();

        try
        {
            var replacementData = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };
            File.WriteAllBytes(tempPath, replacementData);

            // Act
            var result = pck.ReplaceSoundBank(0x12345678, tempPath);

            // Assert
            Assert.True(result);
            Assert.True(pck.SoundBanks[0x12345678]!.IsModified);
            Assert.Equal(replacementData, pck.SoundBanks[0x12345678]!.GetData());
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void ReplaceSoundBankByName_ReplacesMatchingEntry()
    {
        // Arrange
        var pck = CreateTestPckFileWithNames();
        var replacementData = new byte[] { 0x11, 0x22, 0x33, 0x44 };

        // Act
        var count = pck.ReplaceSoundBankByName("TestBank", replacementData);

        // Assert
        Assert.Equal(1, count);

        var entry = pck.SoundBanks[0x12345678];
        Assert.NotNull(entry);
        Assert.True(entry.IsModified);
        Assert.Equal(replacementData, entry.GetData());
    }

    [Fact]
    public void ReplaceSoundBankByName_ReturnsZero_WhenNameNotFound()
    {
        // Arrange
        var pck = CreateTestPckFileWithNames();

        // Act
        var count = pck.ReplaceSoundBankByName("NonExistent", [0x01]);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void ReplaceSoundBankByName_IsCaseInsensitive()
    {
        // Arrange
        var pck = CreateTestPckFileWithNames();

        // Act
        var count = pck.ReplaceSoundBankByName("TESTBANK", [0x01, 0x02]);

        // Assert
        Assert.Equal(1, count);
    }

    [Fact]
    public void ReplaceSoundBankByName_WithLanguageId_OnlyReplacesMatchingLanguage()
    {
        // Arrange
        var pck = CreateTestPckFileWithMultipleLanguages();
        var replacementData = new byte[] { 0xFF, 0xEE, 0xDD };

        // Act
        var count = pck.ReplaceSoundBankByName("TestBank", replacementData, 1);

        // Assert
        Assert.Equal(1, count);

        // Language 1 should be modified
        var lang1Entries = pck.GetSoundBanksByName("TestBank", 1).ToList();
        Assert.Single(lang1Entries);
        Assert.True(lang1Entries[0].IsModified);

        // Language 2 should not be modified
        var lang2Entries = pck.GetSoundBanksByName("TestBank", 2).ToList();
        Assert.Single(lang2Entries);
        Assert.False(lang2Entries[0].IsModified);
    }

#endregion

#region GetSoundBank Tests

    [Fact]
    public void GetSoundBank_ReturnsEntry_WhenExists()
    {
        // Arrange
        var pck = CreateTestPckFile();

        // Act
        var entry = pck.GetSoundBank(0x12345678);

        // Assert
        Assert.NotNull(entry);
        Assert.Equal(0x12345678u, entry.Id);
    }

    [Fact]
    public void GetSoundBank_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var pck = CreateTestPckFile();

        // Act
        var entry = pck.GetSoundBank(0xDEADBEEF);

        // Assert
        Assert.Null(entry);
    }

    [Fact]
    public void GetSoundBanksByName_ReturnsMatchingEntries()
    {
        // Arrange
        var pck = CreateTestPckFileWithNames();

        // Act
        var entries = pck.GetSoundBanksByName("TestBank").ToList();

        // Assert
        Assert.Single(entries);
        Assert.Equal("TestBank", entries[0].Name);
    }

    [Fact]
    public void GetSoundBanksByName_WithLanguageFilter_FiltersCorrectly()
    {
        // Arrange
        var pck = CreateTestPckFileWithMultipleLanguages();

        // Act
        var entries = pck.GetSoundBanksByName("TestBank", 1).ToList();

        // Assert
        Assert.Single(entries);
        Assert.Equal(1u, entries[0].LanguageId);
    }

#endregion

#region HasModifications Tests

    [Fact]
    public void HasModifications_ReturnsTrue_AfterReplacement()
    {
        // Arrange
        var pck = CreateTestPckFile();

        // Act
        pck.ReplaceSoundBank(0x12345678, [0x01, 0x02]);

        // Assert
        Assert.True(pck.HasModifications);
    }

    [Fact]
    public void HasModifications_ReturnsFalse_WhenUnmodified()
    {
        // Arrange
        var pck = CreateTestPckFile();

        // Assert
        Assert.False(pck.HasModifications);
    }

#endregion

#region Creation From Scratch Tests

    [Fact]
    public void Create_ShouldReturnEmptyPackage()
    {
        // Act
        var pck = PckFile.Create();

        // Assert
        Assert.NotNull(pck);
        Assert.Empty(pck.SoundBanks);
        Assert.Empty(pck.StreamingFiles);
        Assert.Empty(pck.ExternalFiles);
        Assert.Empty(pck.Languages);
    }

    [Fact]
    public void AddLanguage_ShouldAddLanguageMapping()
    {
        // Arrange
        var pck = PckFile.Create();

        // Act
        pck.AddLanguage(1, "English");
        pck.AddLanguage(2, "French");

        // Assert
        Assert.Equal(2, pck.Languages.Count);
        Assert.Equal("English", pck.Languages[1]);
        Assert.Equal("French", pck.Languages[2]);
    }

    [Fact]
    public void AddSoundBank_WithBytes_ShouldAddEntry()
    {
        // Arrange
        var pck = PckFile.Create();
        var testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };

        // Act
        var entry = pck.AddSoundBank(0x12345678, testData, 1, name: "TestBank");

        // Assert
        Assert.Single(pck.SoundBanks);
        Assert.Equal(0x12345678u, entry.Id);
        Assert.Equal(1u, entry.LanguageId);
        Assert.Equal("TestBank", entry.Name);
        Assert.Equal(testData, entry.GetData());
    }

    [Fact]
    public void AddSoundBank_ShouldResolveLanguageName()
    {
        // Arrange
        var pck = PckFile.Create();
        pck.AddLanguage(1, "English");
        var testData = new byte[] { 0x01, 0x02, 0x03 };

        // Act
        var entry = pck.AddSoundBank(0x12345678, testData, 1);

        // Assert
        Assert.Equal("English", entry.Language);
    }

    [Fact]
    public void AddStreamingFile_WithBytes_ShouldAddEntry()
    {
        // Arrange
        var pck = PckFile.Create();
        var testData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        // Act
        var entry = pck.AddStreamingFile(0xCAFEBABE, testData, name: "TestStream");

        // Assert
        Assert.Single(pck.StreamingFiles);
        Assert.Equal(0xCAFEBABEu, entry.Id);
        Assert.Equal("TestStream", entry.Name);
        Assert.Equal(testData, entry.GetData());
    }

    [Fact]
    public void AddExternalFile_WithBytes_ShouldAddEntry()
    {
        // Arrange
        var pck = PckFile.Create();
        var testData = new byte[] { 0x11, 0x22, 0x33, 0x44 };

        // Act
        var entry = pck.AddExternalFile(0xDEADBEEFCAFEBABE, testData, name: "TestExternal");

        // Assert
        Assert.Single(pck.ExternalFiles);
        Assert.Equal(0xDEADBEEFCAFEBABEul, entry.Id);
        Assert.Equal("TestExternal", entry.Name);
        Assert.Equal(testData, entry.GetData());
    }

#endregion

#region Round-Trip Tests

    [Fact]
    public void Create_SaveAndLoad_RoundTrip()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        PckFile? loaded = null;

        try
        {
            var pck = PckFile.Create();
            pck.AddLanguage(0, "SFX");
            pck.AddLanguage(1, "English");

            var bankData = new byte[] { 0x42, 0x4B, 0x48, 0x44, 0x00, 0x00, 0x00, 0x14 }; // Minimal BKHD header
            pck.AddSoundBank(0x12345678, bankData, name: "TestBank");

            var streamData = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // RIFF header
            pck.AddStreamingFile(0xABCDEF01, streamData, 1, name: "TestStream");

            // Act
            pck.Save(tempPath);
            loaded = PckFile.Load(tempPath);

            // Assert
            Assert.NotNull(loaded);
            Assert.Equal(2, loaded.Languages.Count);
            Assert.Equal("SFX", loaded.Languages[0]);
            Assert.Equal("English", loaded.Languages[1]);

            Assert.Single(loaded.SoundBanks);
            Assert.Equal(0x12345678u, loaded.SoundBanks.Entries[0].Id);
            Assert.Equal(bankData, loaded.SoundBanks.Entries[0].GetData());

            Assert.Single(loaded.StreamingFiles);
            Assert.Equal(0xABCDEF01u, loaded.StreamingFiles.Entries[0].Id);
            Assert.Equal(streamData, loaded.StreamingFiles.Entries[0].GetData());
        }
        finally
        {
            loaded?.Dispose();
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    private const string SoundsPckPath =
        @"C:\Program Files (x86)\Steam\steamapps\common\HaloWarsDE\sound\wwise_2013\GeneratedSoundBanks\Windows\Sounds.pck";

    [SkippableFact]
    public void RoundTrip_SaveAndLoadProducesIdenticalData()
    {
        Skip.IfNot(File.Exists(SoundsPckPath), $"Sounds.pck not found at {SoundsPckPath}");

        // Load the original PCK
        using var original = PckFile.Load(SoundsPckPath);
        Assert.NotNull(original);

        // Save to temp file
        var tempPath = Path.GetTempFileName();

        try
        {
            original.Save(tempPath);

            // Load the saved file
            using var reloaded = PckFile.Load(tempPath);
            Assert.NotNull(reloaded);

            // Use equality operator - should be identical
            Assert.True(original == reloaded, "Round-trip produced different data (equality operator)");
            Assert.Equal(original, reloaded);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

#endregion

#region Helper Methods

    private static PckFile CreateTestPckFile()
    {
        var pck = new PckFile();
        var entry = new SoundBankEntry { Id = 0x12345678, LanguageId = 0, BlockSize = 16 };
        entry.SetOriginalData([0x01, 0x02, 0x03, 0x04]);
        pck.SoundBanks.Add(entry);

        return pck;
    }

    private static PckFile CreateTestPckFileWithNames()
    {
        var pck = new PckFile();
        var entry = new SoundBankEntry { Id = 0x12345678, LanguageId = 0, BlockSize = 16, Name = "TestBank" };
        entry.SetOriginalData([0x01, 0x02, 0x03, 0x04]);
        pck.SoundBanks.Add(entry);

        return pck;
    }

    private static PckFile CreateTestPckFileWithMultipleLanguages()
    {
        var pck = new PckFile();

        var entry1 = new SoundBankEntry { Id = 0x12345678, LanguageId = 1, BlockSize = 16, Name = "TestBank" };
        entry1.SetOriginalData([0x01, 0x02, 0x03, 0x04]);
        pck.SoundBanks.Add(entry1);

        var entry2 = new SoundBankEntry { Id = 0x87654321, LanguageId = 2, BlockSize = 16, Name = "TestBank" };
        entry2.SetOriginalData([0x05, 0x06, 0x07, 0x08]);
        pck.SoundBanks.Add(entry2);

        return pck;
    }

#endregion
}

