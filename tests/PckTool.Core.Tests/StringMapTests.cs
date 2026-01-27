using PckTool.Core.WWise.Pck;

namespace PckTool.Core.Tests;

public class StringMapTests
{
#region Round-trip Tests

    [Fact]
    public void WriteAndRead_ShouldPreserveData()
    {
        var original = new StringMap();
        original.Map[0x00000001u] = "English";
        original.Map[0x00000002u] = "Japanese";
        original.Map[0x00000003u] = "German";

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var size = original.Write(writer);

        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        var loaded = new StringMap();
        var result = loaded.Read(reader, size);

        Assert.True(result);
        Assert.Equal(original.Map.Count, loaded.Map.Count);

        foreach (var (key, value) in original.Map)
        {
            Assert.True(loaded.Map.ContainsKey(key));
            Assert.Equal(value, loaded.Map[key]);
        }
    }

#endregion

#region Read Tests

    [Fact]
    public void Read_EmptyMap_ShouldSucceed()
    {
        var stringMap = new StringMap();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Write count = 0
        writer.Write(0u);

        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        var result = stringMap.Read(reader, 4);

        Assert.True(result);
        Assert.Empty(stringMap.Map);
    }

    [Fact]
    public void Read_SingleEntry_ShouldReadCorrectly()
    {
        var stringMap = new StringMap();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Count
        writer.Write(1u);

        // Entry: offset (relative to start), id
        // Offset = 4 (count) + 8 (one entry) = 12
        writer.Write(12u);         // offset
        writer.Write(0x12345678u); // id

        // String: "EN" as wide string with null terminator
        writer.Write((ushort) 'E');
        writer.Write((ushort) 'N');
        writer.Write((ushort) 0); // null terminator

        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        var result = stringMap.Read(reader, (uint) stream.Length);

        Assert.True(result);
        Assert.Single(stringMap.Map);
        Assert.Equal("EN", stringMap.Map[0x12345678u]);
    }

    [Fact]
    public void Read_MultipleEntries_ShouldReadAllCorrectly()
    {
        var stringMap = new StringMap();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Count = 2
        writer.Write(2u);

        // Calculate offsets:
        // Header: 4 bytes (count) + 2*8 bytes (entries) = 20 bytes
        // First string at offset 20
        // "EN" = 3 wide chars (including null) = 6 bytes
        // Second string at offset 26

        // Entry 1: offset, id
        writer.Write(20u);         // offset to "EN"
        writer.Write(0x00000001u); // id

        // Entry 2: offset, id  
        writer.Write(26u);         // offset to "JP"
        writer.Write(0x00000002u); // id

        // String 1: "EN"
        writer.Write((ushort) 'E');
        writer.Write((ushort) 'N');
        writer.Write((ushort) 0);

        // String 2: "JP"
        writer.Write((ushort) 'J');
        writer.Write((ushort) 'P');
        writer.Write((ushort) 0);

        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        var result = stringMap.Read(reader, (uint) stream.Length);

        Assert.True(result);
        Assert.Equal(2, stringMap.Map.Count);
        Assert.Equal("EN", stringMap.Map[0x00000001u]);
        Assert.Equal("JP", stringMap.Map[0x00000002u]);
    }

#endregion

#region Write Tests

    [Fact]
    public void Write_EmptyMap_ShouldWriteCountOnly()
    {
        var stringMap = new StringMap();
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        var bytesWritten = stringMap.Write(writer);

        Assert.Equal(4u, bytesWritten); // Only count (0)

        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        Assert.Equal(0u, reader.ReadUInt32());
    }

    [Fact]
    public void Write_SingleEntry_ShouldWriteCorrectly()
    {
        var stringMap = new StringMap();
        stringMap.Map[0xABCDEF00u] = "Test";

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        var bytesWritten = stringMap.Write(writer);

        // 4 (count) + 8 (entry) + 10 (5 wide chars including null) = 22
        Assert.Equal(22u, bytesWritten);
    }

#endregion
}
