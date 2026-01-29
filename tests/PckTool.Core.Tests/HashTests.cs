using PckTool.Core.WWise.Common;

namespace PckTool.Core.Tests;

public class HashTests
{
#region AkmmioFourcc Tests

    [Fact]
    public void AkmmioFourcc_BKHD_ShouldReturnCorrectValue()
    {
        // BKHD = Bank Header chunk ID
        var result = Hash.AkmmioFourcc('B', 'K', 'H', 'D');

        // 'B' = 0x42, 'K' = 0x4B, 'H' = 0x48, 'D' = 0x44
        // Little-endian: 0x44484B42
        Assert.Equal(0x44484B42u, result);
    }

    [Fact]
    public void AkmmioFourcc_AKPK_ShouldReturnCorrectValue()
    {
        // AKPK = Package header
        var result = Hash.AkmmioFourcc('A', 'K', 'P', 'K');

        // 'A' = 0x41, 'K' = 0x4B, 'P' = 0x50, 'K' = 0x4B
        // Little-endian: 0x4B504B41
        Assert.Equal(0x4B504B41u, result);
    }

    [Fact]
    public void AkmmioFourcc_HIRC_ShouldReturnCorrectValue()
    {
        // HIRC = Hierarchy chunk
        var result = Hash.AkmmioFourcc('H', 'I', 'R', 'C');

        // 'H' = 0x48, 'I' = 0x49, 'R' = 0x52, 'C' = 0x43
        // Little-endian: 0x43524948
        Assert.Equal(0x43524948u, result);
    }

    [Fact]
    public void AkmmioFourcc_DIDX_ShouldReturnCorrectValue()
    {
        // DIDX = Data Index chunk
        var result = Hash.AkmmioFourcc('D', 'I', 'D', 'X');

        // 'D' = 0x44, 'I' = 0x49, 'D' = 0x44, 'X' = 0x58
        // Little-endian: 0x58444944
        Assert.Equal(0x58444944u, result);
    }

    [Fact]
    public void AkmmioFourcc_DATA_ShouldReturnCorrectValue()
    {
        // DATA = Data chunk
        var result = Hash.AkmmioFourcc('D', 'A', 'T', 'A');

        // 'D' = 0x44, 'A' = 0x41, 'T' = 0x54, 'A' = 0x41
        // Little-endian: 0x41544144
        Assert.Equal(0x41544144u, result);
    }

    [Fact]
    public void AkmmioFourcc_PLAT_ShouldReturnCorrectValue()
    {
        // PLAT = Platform chunk
        var result = Hash.AkmmioFourcc('P', 'L', 'A', 'T');

        // 'P' = 0x50, 'L' = 0x4C, 'A' = 0x41, 'T' = 0x54
        // Little-endian: 0x54414C50
        Assert.Equal(0x54414C50u, result);
    }

#endregion

#region Fnv132 Tests

    [Fact]
    public void Fnv132_EmptyString_ShouldReturnZero()
    {
        var result = Hash.Fnv132("");

        Assert.Equal(0u, result);
    }

    [Fact]
    public void Fnv132_NullString_ShouldReturnZero()
    {
        var result = Hash.Fnv132(null!);

        Assert.Equal(0u, result);
    }

    [Fact]
    public void Fnv132_SameStringDifferentCase_ShouldReturnSameHash()
    {
        // FNV1 uses lowercase conversion
        var lower = Hash.Fnv132("test");
        var upper = Hash.Fnv132("TEST");
        var mixed = Hash.Fnv132("TeSt");

        Assert.Equal(lower, upper);
        Assert.Equal(lower, mixed);
    }

    [Fact]
    public void Fnv132_KnownValue_ShouldReturnExpectedHash()
    {
        // Standard FNV-1 hash for "a" (lowercase)
        // Offset basis: 0x811C9DC5
        // After 'a' (0x61): ((0x811C9DC5 * 0x1000193) ^ 0x61)
        var result = Hash.Fnv132("a");

        // Calculate expected: ((0x811C9DC5 * 0x1000193) & 0xFFFFFFFF) ^ 0x61
        Assert.NotEqual(0u, result);
    }

    [Fact]
    public void Fnv132_DifferentStrings_ShouldReturnDifferentHashes()
    {
        var hash1 = Hash.Fnv132("sound1");
        var hash2 = Hash.Fnv132("sound2");
        var hash3 = Hash.Fnv132("music");

        Assert.NotEqual(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
        Assert.NotEqual(hash2, hash3);
    }

#endregion

#region GetIdFromString Tests

    [Fact]
    public void GetIdFromString_EmptyString_ShouldReturnZero()
    {
        var result = Hash.GetIdFromString("");

        Assert.Equal(0u, result);
    }

    [Fact]
    public void GetIdFromString_NullString_ShouldReturnZero()
    {
        var result = Hash.GetIdFromString(null!);

        Assert.Equal(0u, result);
    }

    [Fact]
    public void GetIdFromString_WithExtension_ShouldRemoveExtension()
    {
        // "test.wem" should hash as "test"
        var withExt = Hash.GetIdFromString("test.wem");
        var withoutExt = Hash.GetIdFromString("test");

        Assert.Equal(withoutExt, withExt);
    }

    [Fact]
    public void GetIdFromString_WithBnkExtension_ShouldRemoveExtension()
    {
        var withExt = Hash.GetIdFromString("soundbank.bnk");
        var withoutExt = Hash.GetIdFromString("soundbank");

        Assert.Equal(withoutExt, withExt);
    }

    [Fact]
    public void GetIdFromString_NoExtension_ShouldHashFullString()
    {
        var result1 = Hash.GetIdFromString("mysound");
        var result2 = Hash.Fnv132("mysound");

        Assert.Equal(result2, result1);
    }

#endregion
}
