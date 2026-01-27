using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Tests;

public class FileEntryTests
{
    private static SoundBankEntry CreateTestEntry(uint id = 0x12345678, uint blockSize = 16)
    {
        return new SoundBankEntry { Id = id, LanguageId = 0, BlockSize = blockSize };
    }

#region Basic Properties Tests

    [Fact]
    public void NewEntry_ShouldNotBeModified()
    {
        var entry = CreateTestEntry();

        Assert.False(entry.IsModified);
    }

    [Fact]
    public void Entry_WithOriginalData_ShouldReturnData()
    {
        var entry = CreateTestEntry();
        var testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        entry.SetOriginalData(testData);

        var data = entry.GetData();

        Assert.Equal(testData, data);
    }

    [Fact]
    public void Entry_WithNoData_ShouldReturnEmptyArray()
    {
        var entry = CreateTestEntry();

        var data = entry.GetData();

        Assert.Empty(data);
    }

    [Fact]
    public void Size_ShouldReturnDataLength()
    {
        var entry = CreateTestEntry();
        var testData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        entry.SetOriginalData(testData);

        Assert.Equal(5, entry.Size);
    }

#endregion

#region Replacement Tests

    [Fact]
    public void ReplaceWith_Bytes_ShouldMarkAsModified()
    {
        var entry = CreateTestEntry();
        entry.SetOriginalData([0x01, 0x02]);

        entry.ReplaceWith([0xAA, 0xBB, 0xCC]);

        Assert.True(entry.IsModified);
    }

    [Fact]
    public void ReplaceWith_Bytes_ShouldReturnNewData()
    {
        var entry = CreateTestEntry();
        entry.SetOriginalData([0x01, 0x02]);
        var replacement = new byte[] { 0xAA, 0xBB, 0xCC };

        entry.ReplaceWith(replacement);
        var data = entry.GetData();

        Assert.Equal(replacement, data);
    }

    [Fact]
    public void ReplaceWith_FilePath_ShouldMarkAsModified()
    {
        var entry = CreateTestEntry();
        entry.SetOriginalData([0x01, 0x02]);

        // Just checking the modified flag - won't actually read the file
        entry.ReplaceWith("dummy/path.wem");

        Assert.True(entry.IsModified);
    }

    [Fact]
    public void ReplaceWith_Bytes_ThenFilePath_ShouldUseFilePath()
    {
        var entry = CreateTestEntry();
        entry.ReplaceWith([0x01, 0x02]);

        // Create a temp file for testing
        var tempPath = Path.GetTempFileName();

        try
        {
            File.WriteAllBytes(tempPath, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
            entry.ReplaceWith(tempPath);

            var data = entry.GetData();

            Assert.Equal([0xDE, 0xAD, 0xBE, 0xEF], data);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

#endregion

#region Revert Tests

    [Fact]
    public void Revert_ShouldClearModifiedState()
    {
        var entry = CreateTestEntry();
        entry.SetOriginalData([0x01, 0x02]);
        entry.ReplaceWith([0xAA, 0xBB]);

        entry.Revert();

        Assert.False(entry.IsModified);
    }

    [Fact]
    public void Revert_ShouldRestoreOriginalData()
    {
        var entry = CreateTestEntry();
        var original = new byte[] { 0x01, 0x02, 0x03 };
        entry.SetOriginalData(original);
        entry.ReplaceWith([0xAA, 0xBB]);

        entry.Revert();
        var data = entry.GetData();

        Assert.Equal(original, data);
    }

#endregion

#region AlignedSize Tests

    [Fact]
    public void AlignedSize_WhenAlreadyAligned_ShouldEqualSize()
    {
        var entry = CreateTestEntry(blockSize: 16);
        entry.SetOriginalData(new byte[32]); // 32 is divisible by 16

        Assert.Equal(32, entry.AlignedSize);
    }

    [Fact]
    public void AlignedSize_WhenNotAligned_ShouldRoundUp()
    {
        var entry = CreateTestEntry(blockSize: 16);
        entry.SetOriginalData(new byte[20]); // 20 is not divisible by 16

        Assert.Equal(32, entry.AlignedSize); // Next multiple of 16
    }

    [Fact]
    public void AlignedSize_WithBlockSizeZero_ShouldEqualSize()
    {
        var entry = CreateTestEntry(blockSize: 0);
        entry.SetOriginalData(new byte[17]);

        Assert.Equal(17, entry.AlignedSize);
    }

#endregion

#region IsValid Tests

    [Fact]
    public void IsValid_WithData_ShouldReturnTrue()
    {
        var entry = CreateTestEntry();
        entry.SetOriginalData([0x01]);

        Assert.True(entry.IsValid);
    }

    [Fact]
    public void IsValid_WithNoData_ShouldReturnFalse()
    {
        var entry = CreateTestEntry();

        Assert.False(entry.IsValid);
    }

#endregion

#region PropertyChanged Tests

    [Fact]
    public void ReplaceWith_ShouldRaisePropertyChanged()
    {
        var entry = CreateTestEntry();
        var changedProperties = new List<string>();
        entry.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        entry.ReplaceWith([0x01]);

        Assert.Contains("IsModified", changedProperties);
        Assert.Contains("Size", changedProperties);
    }

    [Fact]
    public void Revert_ShouldRaisePropertyChanged()
    {
        var entry = CreateTestEntry();
        entry.ReplaceWith([0x01]);

        var changedProperties = new List<string>();
        entry.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName!);

        entry.Revert();

        Assert.Contains("IsModified", changedProperties);
        Assert.Contains("Size", changedProperties);
    }

#endregion
}
