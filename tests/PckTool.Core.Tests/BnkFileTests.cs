using PckTool.Abstractions;
using PckTool.Core.WWise;
using PckTool.Core.WWise.Bnk;

namespace PckTool.Core.Tests;

public class BnkFileTests
{
#region Construction Tests

    [Fact]
    public void Constructor_WithSoundBank_SetsProperties()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678, 5) { Version = 0x71 };

        // Act
        var bnkFile = new BnkFile(soundBank);

        // Assert
        Assert.Same(soundBank, bnkFile.SoundBank);
        Assert.Null(bnkFile.SourcePath);
        Assert.False(bnkFile.HasModifications);
        Assert.Equal(AudioFileType.Bnk, bnkFile.FileType);
    }

    [Fact]
    public void Constructor_WithSourcePath_SetsSourcePath()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678);

        // Act
        var bnkFile = new BnkFile(soundBank, "test.bnk");

        // Assert
        Assert.Equal("test.bnk", bnkFile.SourcePath);
    }

    [Fact]
    public void Constructor_WithNullSoundBank_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BnkFile(null!));
    }

#endregion

#region IAudioFile Properties Tests

    [Fact]
    public void SoundBankCount_ReturnsOne()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Assert
        Assert.Equal(1, bnkFile.SoundBankCount);
    }

    [Fact]
    public void StreamingFileCount_ReturnsZero()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Assert
        Assert.Equal(0, bnkFile.StreamingFileCount);
    }

    [Fact]
    public void ExternalFileCount_ReturnsZero()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Assert
        Assert.Equal(0, bnkFile.ExternalFileCount);
    }

    [Fact]
    public void Languages_ReturnsSfxMapping()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Assert
        Assert.Single(bnkFile.Languages);
        Assert.Equal("SFX", bnkFile.Languages[0]);
    }

    [Fact]
    public void SoundBanks_ReturnsSingleEntryCollection()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678);
        var bnkFile = new BnkFile(soundBank);

        // Assert
        Assert.Equal(1, bnkFile.SoundBanks.Count);
        Assert.NotNull(bnkFile.SoundBanks[0x12345678]);
    }

    [Fact]
    public void StreamingFiles_ReturnsEmptyCollection()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Assert
        Assert.Equal(0, bnkFile.StreamingFiles.Count);
    }

    [Fact]
    public void ExternalFiles_ReturnsEmptyCollection()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Assert
        Assert.Equal(0, bnkFile.ExternalFiles.Count);
    }

#endregion

#region WEM Operations Tests

    [Fact]
    public void ContainsWem_ReturnsTrueForExistingMedia()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678);
        soundBank.Media.Add(100, [0x01, 0x02, 0x03]);
        var bnkFile = new BnkFile(soundBank);

        // Act & Assert
        Assert.True(bnkFile.ContainsWem(100));
    }

    [Fact]
    public void ContainsWem_ReturnsFalseForNonExistentMedia()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Act & Assert
        Assert.False(bnkFile.ContainsWem(999));
    }

    [Fact]
    public void FindWem_ReturnsDataForExistingMedia()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678);
        var wemData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        soundBank.Media.Add(100, wemData);
        var bnkFile = new BnkFile(soundBank);

        // Act
        var result = bnkFile.FindWem(100);

        // Assert
        Assert.Equal(wemData, result);
    }

    [Fact]
    public void FindWem_ReturnsNullForNonExistentMedia()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Act
        var result = bnkFile.FindWem(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ReplaceWem_UpdatesMediaAndMarksModified()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678);
        soundBank.Media.Add(100, [0x01, 0x02]);
        var bnkFile = new BnkFile(soundBank);
        var newData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };

        // Act
        var result = bnkFile.ReplaceWem(100, newData);

        // Assert
        Assert.Equal(100u, result.SourceId);
        Assert.Equal(1, result.EmbeddedBanksModified);
        Assert.True(bnkFile.HasModifications);
        Assert.Equal(newData, bnkFile.FindWem(100));
    }

    [Fact]
    public void ReplaceWem_ReturnsEmptyResultForNonExistentMedia()
    {
        // Arrange
        var bnkFile = new BnkFile(new SoundBank(0x12345678));

        // Act
        var result = bnkFile.ReplaceWem(999, [0x01, 0x02]);

        // Assert
        Assert.Equal(999u, result.SourceId);
        Assert.Equal(0, result.EmbeddedBanksModified);
        Assert.False(bnkFile.HasModifications);
    }

#endregion

#region Save Tests

    [Fact]
    public void Save_WritesToFile()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678) { Version = 0x71 };
        var bnkFile = new BnkFile(soundBank);
        var tempPath = Path.GetTempFileName();

        try
        {
            // Act
            bnkFile.Save(tempPath);

            // Assert
            Assert.True(File.Exists(tempPath));
            var bytes = File.ReadAllBytes(tempPath);
            Assert.True(bytes.Length > 0);
            Assert.Equal((byte) 'B', bytes[0]);
            Assert.Equal((byte) 'K', bytes[1]);
            Assert.Equal((byte) 'H', bytes[2]);
            Assert.Equal((byte) 'D', bytes[3]);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void Save_WritesToStream()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678) { Version = 0x71 };
        var bnkFile = new BnkFile(soundBank);

        // Act
        using var stream = new MemoryStream();
        bnkFile.Save(stream);

        // Assert
        Assert.True(stream.Length > 0);
        stream.Position = 0;
        Assert.Equal((byte) 'B', stream.ReadByte());
        Assert.Equal((byte) 'K', stream.ReadByte());
    }

    [Fact]
    public void RoundTrip_PreservesData()
    {
        // Arrange
        var original = new SoundBank(0x12345678, 5) { Version = 0x71, ProjectId = 42 };
        original.Media.Add(100, [0x01, 0x02, 0x03, 0x04]);
        var bnkFile = new BnkFile(original);
        var tempPath = Path.GetTempFileName();

        try
        {
            // Act
            bnkFile.Save(tempPath);
            var loaded = BnkFile.Load(tempPath);

            // Assert
            Assert.Equal(original.Id, loaded.SoundBank.Id);
            Assert.Equal(original.LanguageId, loaded.SoundBank.LanguageId);
            Assert.Equal(original.Version, loaded.SoundBank.Version);
            Assert.True(loaded.ContainsWem(100));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

#endregion

#region Load Tests

    [Fact]
    public void Load_ThrowsForNonExistentFile()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => BnkFile.Load("nonexistent.bnk"));
    }

    [Fact]
    public void Load_SetsSourcePath()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678) { Version = 0x71 };
        var tempPath = Path.GetTempFileName();

        try
        {
            soundBank.Save(tempPath);

            // Act
            var loaded = BnkFile.Load(tempPath);

            // Assert
            Assert.Equal(tempPath, loaded.SourcePath);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

#endregion
}

public class AudioFileFactoryTests
{
#region DetectFileType Tests

    [Theory]
    [InlineData("test.pck", AudioFileType.Pck)]
    [InlineData("test.PCK", AudioFileType.Pck)]
    [InlineData("test.bnk", AudioFileType.Bnk)]
    [InlineData("test.BNK", AudioFileType.Bnk)]
    public void DetectFileType_ReturnsCorrectType(string path, AudioFileType expected)
    {
        // Arrange
        var factory = new AudioFileFactory();

        // Act
        var result = factory.DetectFileType(path);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DetectFileType_ThrowsForUnknownExtension()
    {
        // Arrange
        var factory = new AudioFileFactory();

        // Act & Assert
        Assert.Throws<InvalidDataException>(() => factory.DetectFileType("test.wav"));
    }

#endregion

#region IsSupportedExtension Tests

    [Theory]
    [InlineData(".pck", true)]
    [InlineData(".PCK", true)]
    [InlineData(".bnk", true)]
    [InlineData(".BNK", true)]
    [InlineData("pck", true)]
    [InlineData("bnk", true)]
    [InlineData(".wav", false)]
    [InlineData(".mp3", false)]
    public void IsSupportedExtension_ReturnsCorrectResult(string extension, bool expected)
    {
        // Arrange
        var factory = new AudioFileFactory();

        // Act
        var result = factory.IsSupportedExtension(extension);

        // Assert
        Assert.Equal(expected, result);
    }

#endregion

#region Load Tests

    [Fact]
    public void Load_ThrowsForNonExistentFile()
    {
        // Arrange
        var factory = new AudioFileFactory();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => factory.Load("nonexistent.pck"));
    }

    [Fact]
    public void Load_ReturnsBnkFileForBnkExtension()
    {
        // Arrange
        var factory = new AudioFileFactory();
        var soundBank = new SoundBank(0x12345678) { Version = 0x71 };
        var tempPath = Path.ChangeExtension(Path.GetTempFileName(), ".bnk");

        try
        {
            soundBank.Save(tempPath);

            // Act
            using var audioFile = factory.Load(tempPath);

            // Assert
            Assert.Equal(AudioFileType.Bnk, audioFile.FileType);
            Assert.IsType<BnkFile>(audioFile);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void Load_FromStream_ReturnsBnkFile()
    {
        // Arrange
        var factory = new AudioFileFactory();
        var soundBank = new SoundBank(0x12345678) { Version = 0x71 };
        var bytes = soundBank.ToByteArray();

        // Act
        using var stream = new MemoryStream(bytes);
        using var audioFile = factory.Load(stream, AudioFileType.Bnk);

        // Assert
        Assert.Equal(AudioFileType.Bnk, audioFile.FileType);
        Assert.IsType<BnkFile>(audioFile);
    }

#endregion

#region IAudioFile Polymorphism Tests

    [Fact]
    public void IAudioFile_BnkFile_CanBeUsedPolymorphically()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678) { Version = 0x71 };
        soundBank.Media.Add(100, [0x01, 0x02, 0x03]);
        IAudioFile audioFile = new BnkFile(soundBank);

        // Act & Assert - All IAudioFile operations work
        Assert.Equal(AudioFileType.Bnk, audioFile.FileType);
        Assert.Equal(1, audioFile.SoundBankCount);
        Assert.Equal(0, audioFile.StreamingFileCount);
        Assert.Equal(0, audioFile.ExternalFileCount);
        Assert.True(audioFile.ContainsWem(100));
        Assert.NotNull(audioFile.FindWem(100));
        Assert.Single(audioFile.Languages);
        Assert.Single(audioFile.SoundBanks);
        Assert.Empty(audioFile.StreamingFiles);
        Assert.Empty(audioFile.ExternalFiles);
    }

    [Fact]
    public void IAudioFile_ReplaceWem_WorksPolymorphically()
    {
        // Arrange
        var soundBank = new SoundBank(0x12345678);
        soundBank.Media.Add(100, [0x01, 0x02]);
        IAudioFile audioFile = new BnkFile(soundBank);
        var newData = new byte[] { 0xAA, 0xBB, 0xCC };

        // Act
        var result = audioFile.ReplaceWem(100, newData);

        // Assert
        Assert.Equal(100u, result.SourceId);
        Assert.Equal(1, result.EmbeddedBanksModified);
        Assert.True(audioFile.HasModifications);
    }

#endregion
}
