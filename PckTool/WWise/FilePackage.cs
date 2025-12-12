using System.Numerics;

namespace PckTool.WWise;

public class FilePackage(string fileName) : IDisposable
{
    private const uint ValidVersion = 0x1;
    private readonly BinaryReader _reader = new(File.OpenRead(fileName));

    private readonly uint _validHeaderTag = Hash.AkmmioFourcc('A', 'K', 'P', 'K');
    public Dictionary<uint, string> LanguageMap { get; private set; } = new();
    public FilePackageLut<uint> SoundBanksLut { get; private set; } = new();
    public FilePackageLut<uint> StmFilesLut { get; private set; } = new();
    public FilePackageLut<ulong> ExternalLuts { get; private set; } = new();

    public void Dispose()
    {
        _reader.Dispose();
    }

    public bool Load()
    {
        var tag = _reader.ReadUInt32();
        var headerSize = _reader.ReadUInt32();

        if (tag != _validHeaderTag || headerSize == 0x0)
        {
            return false;
        }

        var version = _reader.ReadUInt32();

        if (version != ValidVersion)
        {
            return false;
        }

        var languageMapSize = _reader.ReadUInt32();
        var soundBanksLutSize = _reader.ReadUInt32();
        var stmFilesLutSize = _reader.ReadUInt32();
        var externalLutsSize = _reader.ReadUInt32();

        if (headerSize < 24 + languageMapSize + soundBanksLutSize + stmFilesLutSize)
        {
            return false;
        }

        var languageMap = new StringMap();

        if (!languageMap.Read(_reader, languageMapSize))
        {
            return false;
        }

        var soundBanks = new FilePackageLut<uint>();

        if (!soundBanks.Read(_reader, soundBanksLutSize))
        {
            return false;
        }

        var stmFiles = new FilePackageLut<uint>();

        if (!stmFiles.Read(_reader, stmFilesLutSize))
        {
            return false;
        }

        var externalLuts = new FilePackageLut<ulong>();

        if (!externalLuts.Read(_reader, externalLutsSize))
        {
            return false;
        }

        // expose data
        LanguageMap = languageMap.Map;
        SoundBanksLut = soundBanks;
        StmFilesLut = stmFiles;
        ExternalLuts = externalLuts;

        return true;
    }

    public void Save(string fileName)
    {
        using var writer = new BinaryWriter(File.Create(fileName));

        // Write tag
        writer.Write(_validHeaderTag);

        // Placeholder for header size (will be filled in later)
        var headerSizePosition = writer.BaseStream.Position;
        writer.Write(0u);

        writer.Write(ValidVersion);

        // Placeholders for section sizes (will be filled in later)
        var languageMapSizePosition = writer.BaseStream.Position;
        writer.Write(0u); // language map size
        writer.Write(0u); // sound banks lut size
        writer.Write(0u); // stm files lut size
        writer.Write(0u); // external luts size

        // Write language map
        var stringMap = new StringMap();

        foreach (var (id, name) in LanguageMap)
        {
            stringMap.Map[id] = name;
        }

        var languageMapSize = stringMap.Write(writer);

        // Write LUT headers (without correct StartBlock values yet)
        var soundBanksLutSize = SoundBanksLut.CalculateHeaderSize();
        var stmFilesLutSize = StmFilesLut.CalculateHeaderSize();
        var externalLutsSize = ExternalLuts.CalculateHeaderSize();

        // Calculate where file data will start
        var headerSize = (uint) writer.BaseStream.Position + soundBanksLutSize + stmFilesLutSize + externalLutsSize - 8;
        var dataStartOffset = 8 + headerSize; // 8 = tag (4) + headerSize field (4)

        // Update StartBlock values for all entries
        var currentDataOffset = dataStartOffset;
        UpdateStartBlocks(SoundBanksLut, ref currentDataOffset);
        UpdateStartBlocks(StmFilesLut, ref currentDataOffset);
        UpdateStartBlocks(ExternalLuts, ref currentDataOffset);

        // Now write the LUT headers with correct StartBlock values
        SoundBanksLut.Write(writer);
        StmFilesLut.Write(writer);
        ExternalLuts.Write(writer);

        // Write actual file data
        WriteFileData(writer, SoundBanksLut);
        WriteFileData(writer, StmFilesLut);
        WriteFileData(writer, ExternalLuts);

        // Go back and fill in the sizes
        var endPosition = writer.BaseStream.Position;

        writer.BaseStream.Position = headerSizePosition;
        writer.Write(headerSize);

        writer.BaseStream.Position = languageMapSizePosition;
        writer.Write(languageMapSize);
        writer.Write(soundBanksLutSize);
        writer.Write(stmFilesLutSize);
        writer.Write(externalLutsSize);

        writer.BaseStream.Position = endPosition;
    }

    /// <summary>
    ///     Compares this FilePackage with another and logs all differences.
    ///     Returns true if the packages are identical (data-wise), false otherwise.
    /// </summary>
    public bool Compare(FilePackage other, bool logToConsole = true)
    {
        var differences = new List<string>();
        var warnings = new List<string>();

        // Header-level comparisons
        CompareLanguageMaps(other, differences);

        // LUT count comparisons
        if (SoundBanksLut.Entries.Count != other.SoundBanksLut.Entries.Count)
        {
            differences.Add(
                $"SoundBanks count mismatch: {SoundBanksLut.Entries.Count} vs {other.SoundBanksLut.Entries.Count}");
        }

        if (StmFilesLut.Entries.Count != other.StmFilesLut.Entries.Count)
        {
            differences.Add(
                $"StmFiles count mismatch: {StmFilesLut.Entries.Count} vs {other.StmFilesLut.Entries.Count}");
        }

        if (ExternalLuts.Entries.Count != other.ExternalLuts.Entries.Count)
        {
            differences.Add(
                $"ExternalLuts count mismatch: {ExternalLuts.Entries.Count} vs {other.ExternalLuts.Entries.Count}");
        }

        // Per-entry comparisons
        CompareLut("SoundBanks", SoundBanksLut, other.SoundBanksLut, differences, warnings);
        CompareLut("StmFiles", StmFilesLut, other.StmFilesLut, differences, warnings);
        CompareLut("ExternalLuts", ExternalLuts, other.ExternalLuts, differences, warnings);

        // Log results
        if (logToConsole)
        {
            Log.Info("=== FilePackage Comparison Report ===");
            Log.Info("");

            // Summary counts
            Log.Info("Language Map: {0} entries", LanguageMap.Count);
            Log.Info("SoundBanks: {0} entries", SoundBanksLut.Entries.Count);
            Log.Info("StmFiles: {0} entries", StmFilesLut.Entries.Count);
            Log.Info("ExternalLuts: {0} entries", ExternalLuts.Entries.Count);
            Log.Info("");

            if (warnings.Count > 0)
            {
                Log.Warn("--- Warnings ({0}) ---", warnings.Count);

                foreach (var warning in warnings)
                {
                    Log.Warn("  [WARN] {0}", warning);
                }

                Log.Info("");
            }

            if (differences.Count > 0)
            {
                Log.Error("--- Differences ({0}) ---", differences.Count);

                foreach (var diff in differences)
                {
                    Log.Error("  [DIFF] {0}", diff);
                }

                Log.Info("");
            }

            Log.Info("=== Summary ===");
            Log.Info("Total differences: {0}", differences.Count);
            Log.Info("Total warnings: {0}", warnings.Count);
            Log.Info(
                "Round-trip status: {0}",
                differences.Count == 0 ? "SUCCESS - Data identical" : "FAILED - Data corrupted");

            Log.Info("");
        }

        return differences.Count == 0;
    }

    private static void UpdateStartBlocks<TKey>(FilePackageLut<TKey> lut, ref uint currentOffset)
        where TKey : INumber<TKey>
    {
        foreach (var entry in lut.Entries)
        {
            // Align to block size if needed
            if (entry.BlockSize > 0 && currentOffset % entry.BlockSize != 0)
            {
                currentOffset += entry.BlockSize - currentOffset % entry.BlockSize;
            }

            entry.StartBlock = currentOffset;
            currentOffset += (uint) entry.Data.Length;
        }
    }

    private static void WriteFileData<TKey>(BinaryWriter writer, FilePackageLut<TKey> lut)
        where TKey : INumber<TKey>
    {
        foreach (var entry in lut.Entries)
        {
            // Seek to the correct position (StartBlock is absolute offset)
            writer.BaseStream.Position = entry.StartBlock;

            // Write the file data
            writer.Write(entry.Data);
        }
    }

    private void CompareLanguageMaps(FilePackage other, List<string> differences)
    {
        if (LanguageMap.Count != other.LanguageMap.Count)
        {
            differences.Add($"Language map count mismatch: {LanguageMap.Count} vs {other.LanguageMap.Count}");
        }

        foreach (var (id, name) in LanguageMap)
        {
            if (!other.LanguageMap.TryGetValue(id, out var otherName))
            {
                differences.Add($"Language ID {id} ('{name}') missing in other package");
            }
            else if (name != otherName)
            {
                differences.Add($"Language ID {id} name mismatch: '{name}' vs '{otherName}'");
            }
        }

        foreach (var (id, name) in other.LanguageMap)
        {
            if (!LanguageMap.ContainsKey(id))
            {
                differences.Add($"Language ID {id} ('{name}') extra in other package");
            }
        }
    }

    private static void CompareLut<TKey>(
        string lutName,
        FilePackageLut<TKey> lut,
        FilePackageLut<TKey> other,
        List<string> differences,
        List<string> warnings)
        where TKey : INumber<TKey>
    {
        var minCount = Math.Min(lut.Entries.Count, other.Entries.Count);

        for (var i = 0; i < minCount; i++)
        {
            var entry = lut.Entries[i];
            var otherEntry = other.Entries[i];
            var prefix = $"{lutName}[{i}] (FileId: {entry.FileId:X})";

            // FileId
            if (!entry.FileId.Equals(otherEntry.FileId))
            {
                differences.Add($"{prefix}: FileId mismatch: {entry.FileId:X} vs {otherEntry.FileId:X}");
            }

            // BlockSize
            if (entry.BlockSize != otherEntry.BlockSize)
            {
                differences.Add($"{prefix}: BlockSize mismatch: {entry.BlockSize} vs {otherEntry.BlockSize}");
            }

            // StartBlock (expected to change, so it's a warning)
            if (entry.StartBlock != otherEntry.StartBlock)
            {
                warnings.Add($"{prefix}: StartBlock changed: {entry.StartBlock} -> {otherEntry.StartBlock}");
            }

            // LanguageId
            if (entry.LanguageId != otherEntry.LanguageId)
            {
                differences.Add($"{prefix}: LanguageId mismatch: {entry.LanguageId} vs {otherEntry.LanguageId}");
            }

            // Data length
            if (entry.Data.Length != otherEntry.Data.Length)
            {
                differences.Add($"{prefix}: Data length mismatch: {entry.Data.Length} vs {otherEntry.Data.Length}");
            }
            else
            {
                // Byte-by-byte comparison
                var firstDiffIndex = -1;
                var diffCount = 0;

                for (var j = 0; j < entry.Data.Length; j++)
                {
                    if (entry.Data[j] != otherEntry.Data[j])
                    {
                        if (firstDiffIndex == -1) firstDiffIndex = j;
                        diffCount++;
                    }
                }

                if (diffCount > 0)
                {
                    differences.Add(
                        $"{prefix}: Data content mismatch: {diffCount} bytes differ, first at offset {firstDiffIndex}");
                }
            }

            // IsValid status
            if (!entry.IsValid) differences.Add($"{prefix}: Original entry IsValid = false");

            if (!otherEntry.IsValid) differences.Add($"{prefix}: Reloaded entry IsValid = false");
        }

        // Report extra entries
        for (var i = minCount; i < lut.Entries.Count; i++)
        {
            differences.Add($"{lutName}[{i}] (FileId: {lut.Entries[i].FileId:X}): Missing in reloaded package");
        }

        for (var i = minCount; i < other.Entries.Count; i++)
        {
            differences.Add($"{lutName}[{i}] (FileId: {other.Entries[i].FileId:X}): Extra in reloaded package");
        }
    }
}
