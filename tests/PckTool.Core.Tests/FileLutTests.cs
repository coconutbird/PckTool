using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Tests;

public class FileLutTests
{
#region Enumeration Tests

    [Fact]
    public void GetEnumerator_ShouldIterateAllEntries()
    {
        var lut = new SoundBankLut();
        lut.Add(CreateEntry(0x00000001));
        lut.Add(CreateEntry(0x00000002));

        var ids = lut.Select(e => e.Id).ToList();

        Assert.Equal(2, ids.Count);
        Assert.Contains(0x00000001u, ids);
        Assert.Contains(0x00000002u, ids);
    }

#endregion

    private static SoundBankEntry CreateEntry(uint id)
    {
        var entry = new SoundBankEntry { Id = id, LanguageId = 0, BlockSize = 16 };
        entry.SetOriginalData([0x01, 0x02, 0x03, 0x04]);

        return entry;
    }

#region Add/Remove Tests

    [Fact]
    public void Add_ShouldIncreaseCount()
    {
        var lut = new SoundBankLut();
        var entry = CreateEntry(0x00000001);

        lut.Add(entry);

        Assert.Equal(1, lut.Count);
    }

    [Fact]
    public void Add_MultipleEntries_ShouldPreserveOrder()
    {
        var lut = new SoundBankLut();
        var entry1 = CreateEntry(0x00000001);
        var entry2 = CreateEntry(0x00000002);
        var entry3 = CreateEntry(0x00000003);

        lut.Add(entry1);
        lut.Add(entry2);
        lut.Add(entry3);

        Assert.Equal(3, lut.Entries.Count);
        Assert.Equal(0x00000001u, lut.Entries[0].Id);
        Assert.Equal(0x00000002u, lut.Entries[1].Id);
        Assert.Equal(0x00000003u, lut.Entries[2].Id);
    }

    [Fact]
    public void Remove_ExistingEntry_ShouldReturnTrueAndDecreaseCount()
    {
        var lut = new SoundBankLut();
        var entry = CreateEntry(0x00000001);
        lut.Add(entry);

        var result = lut.Remove(0x00000001);

        Assert.True(result);
        Assert.Equal(0, lut.Count);
    }

    [Fact]
    public void Remove_NonExistentEntry_ShouldReturnFalse()
    {
        var lut = new SoundBankLut();

        var result = lut.Remove(0x00000001);

        Assert.False(result);
    }

    [Fact]
    public void Clear_ShouldRemoveAllEntries()
    {
        var lut = new SoundBankLut();
        lut.Add(CreateEntry(0x00000001));
        lut.Add(CreateEntry(0x00000002));

        lut.Clear();

        Assert.Equal(0, lut.Count);
    }

#endregion

#region Lookup Tests

    [Fact]
    public void Indexer_ExistingId_ShouldReturnEntry()
    {
        var lut = new SoundBankLut();
        var entry = CreateEntry(0x00000001);
        lut.Add(entry);

        var result = lut[0x00000001];

        Assert.NotNull(result);
        Assert.Equal(0x00000001u, result.Id);
    }

    [Fact]
    public void Indexer_NonExistentId_ShouldReturnNull()
    {
        var lut = new SoundBankLut();

        var result = lut[0x00000001];

        Assert.Null(result);
    }

    [Fact]
    public void Contains_ExistingId_ShouldReturnTrue()
    {
        var lut = new SoundBankLut();
        lut.Add(CreateEntry(0x00000001));

        Assert.True(lut.Contains(0x00000001));
    }

    [Fact]
    public void Contains_NonExistentId_ShouldReturnFalse()
    {
        var lut = new SoundBankLut();

        Assert.False(lut.Contains(0x00000001));
    }

    [Fact]
    public void ById_ShouldProvideO1Lookup()
    {
        var lut = new SoundBankLut();
        lut.Add(CreateEntry(0x00000001));
        lut.Add(CreateEntry(0x00000002));

        Assert.True(lut.ById.ContainsKey(0x00000001));
        Assert.True(lut.ById.ContainsKey(0x00000002));
        Assert.False(lut.ById.ContainsKey(0x00000003));
    }

#endregion

#region HasModifications Tests

    [Fact]
    public void HasModifications_NoModifiedEntries_ShouldReturnFalse()
    {
        var lut = new SoundBankLut();
        var entry = CreateEntry(0x00000001);
        entry.SetOriginalData([0x01]);
        lut.Add(entry);

        Assert.False(lut.HasModifications);
    }

    [Fact]
    public void HasModifications_WithModifiedEntry_ShouldReturnTrue()
    {
        var lut = new SoundBankLut();
        var entry = CreateEntry(0x00000001);
        entry.SetOriginalData([0x01]);
        lut.Add(entry);

        entry.ReplaceWith([0x02]);

        Assert.True(lut.HasModifications);
    }

#endregion

#region CalculateHeaderSize Tests

    [Fact]
    public void CalculateHeaderSize_Empty_ShouldReturn4()
    {
        var lut = new SoundBankLut();

        Assert.Equal(4u, lut.CalculateHeaderSize());
    }

    [Fact]
    public void CalculateHeaderSize_WithEntries_ShouldCalculateCorrectly()
    {
        var lut = new SoundBankLut();
        lut.Add(CreateEntry(0x00000001));
        lut.Add(CreateEntry(0x00000002));

        // 4 (count) + 2 * (4 keysize + 16 fixed) = 4 + 2*20 = 44
        Assert.Equal(44u, lut.CalculateHeaderSize());
    }

#endregion
}
