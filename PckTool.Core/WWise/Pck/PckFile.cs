using System.Numerics;

using PckTool.Core.WWise.Util;

namespace PckTool.Core.WWise.Pck;

public class PckFile : IDisposable
{
    private const uint ValidVersion = 0x1;
    private static readonly uint ValidHeaderTag = Hash.AkmmioFourcc('A', 'K', 'P', 'K');

    private BinaryReader? _reader;

    /// <summary>
    ///     The source file path this package was loaded from.
    /// </summary>
    public string? SourcePath { get; private set; }

    /// <summary>
    ///     Language ID to name mapping.
    /// </summary>
    public Dictionary<uint, string> Languages { get; private set; } = new();

    /// <summary>
    ///     Sound bank entries.
    /// </summary>
    public SoundBankLut SoundBanks { get; private set; } = new();

    /// <summary>
    ///     Streaming file entries.
    /// </summary>
    public StreamingFileLut StreamingFiles { get; private set; } = new();

    /// <summary>
    ///     External file entries.
    /// </summary>
    public ExternalFileLut ExternalFiles { get; private set; } = new();

    /// <summary>
    ///     Returns true if any entries have been modified.
    /// </summary>
    public bool HasModifications =>
        SoundBanks.HasModifications || StreamingFiles.HasModifications || ExternalFiles.HasModifications;

    public void Dispose()
    {
        _reader?.Dispose();
        _reader = null;
    }

    /// <summary>
    ///     Loads a package from a file path.
    /// </summary>
    public static PckFile? Load(string path)
    {
        var package = new PckFile();

        if (package.LoadInternal(path))
        {
            return package;
        }

        package.Dispose();

        return null;
    }

    /// <summary>
    ///     Gets the language name for a language ID.
    /// </summary>
    public string? GetLanguageName(uint languageId)
    {
        return Languages.GetValueOrDefault(languageId);
    }

    public void Save(string path)
    {
        using var writer = new BinaryWriter(File.Create(path));

        // Write tag
        writer.Write(ValidHeaderTag);

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

        foreach (var (id, name) in Languages)
        {
            stringMap.Map[id] = name;
        }

        var languageMapSize = stringMap.Write(writer);

        // Write LUT headers (without correct StartBlock values yet)
        var soundBanksLutSize = SoundBanks.CalculateHeaderSize();
        var stmFilesLutSize = StreamingFiles.CalculateHeaderSize();
        var externalLutsSize = ExternalFiles.CalculateHeaderSize();

        // Calculate where file data will start
        var headerSize = (uint) writer.BaseStream.Position + soundBanksLutSize + stmFilesLutSize + externalLutsSize - 8;
        var dataStartOffset = 8 + headerSize; // 8 = tag (4) + headerSize field (4)

        // Update StartBlock values for all entries
        var currentDataOffset = dataStartOffset;
        UpdateStartBlocks(SoundBanks, ref currentDataOffset);
        UpdateStartBlocks(StreamingFiles, ref currentDataOffset);
        UpdateStartBlocks(ExternalFiles, ref currentDataOffset);

        // Now write the LUT headers with correct StartBlock values
        SoundBanks.Write(writer);
        StreamingFiles.Write(writer);
        ExternalFiles.Write(writer);

        // Write actual file data
        WriteFileData(SoundBanks, writer);
        WriteFileData(StreamingFiles, writer);
        WriteFileData(ExternalFiles, writer);

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
    public bool Compare(PckFile other, bool logToConsole = true)
    {
        var differences = new List<string>();
        var warnings = new List<string>();

        // Header-level comparisons
        CompareLanguageMaps(other, differences);

        // LUT count comparisons
        if (SoundBanks.Count != other.SoundBanks.Count)
        {
            differences.Add($"SoundBanks count mismatch: {SoundBanks.Count} vs {other.SoundBanks.Count}");
        }

        if (StreamingFiles.Count != other.StreamingFiles.Count)
        {
            differences.Add($"StreamingFiles count mismatch: {StreamingFiles.Count} vs {other.StreamingFiles.Count}");
        }

        if (ExternalFiles.Count != other.ExternalFiles.Count)
        {
            differences.Add($"ExternalFiles count mismatch: {ExternalFiles.Count} vs {other.ExternalFiles.Count}");
        }

        // Per-entry comparisons
        CompareLut("SoundBanks", SoundBanks, other.SoundBanks, differences, warnings);
        CompareLut("StreamingFiles", StreamingFiles, other.StreamingFiles, differences, warnings);
        CompareLut("ExternalFiles", ExternalFiles, other.ExternalFiles, differences, warnings);

        // Log results
        if (logToConsole)
        {
            Log.Info("=== FilePackage Comparison Report ===");
            Log.Info("");
            Log.Info("Languages: {0} entries", Languages.Count);
            Log.Info("SoundBanks: {0} entries", SoundBanks.Count);
            Log.Info("StreamingFiles: {0} entries", StreamingFiles.Count);
            Log.Info("ExternalFiles: {0} entries", ExternalFiles.Count);
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

    private bool LoadInternal(string path)
    {
        SourcePath = path;
        _reader = new BinaryReader(File.OpenRead(path));

        var tag = _reader.ReadUInt32();
        var headerSize = _reader.ReadUInt32();

        if (tag != ValidHeaderTag || headerSize == 0x0)
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

        var soundBanks = new SoundBankLut();

        if (!soundBanks.Read(_reader, soundBanksLutSize))
        {
            return false;
        }

        var streamingFiles = new StreamingFileLut();

        if (!streamingFiles.Read(_reader, stmFilesLutSize))
        {
            return false;
        }

        var externalFiles = new ExternalFileLut();

        if (!externalFiles.Read(_reader, externalLutsSize))
        {
            return false;
        }

        // Store data
        Languages = languageMap.Map;
        SoundBanks = soundBanks;
        StreamingFiles = streamingFiles;
        ExternalFiles = externalFiles;

        // Resolve language names on entries
        ResolveLanguageNames();

        return true;
    }

    /// <summary>
    ///     Resolves language names on all entries.
    /// </summary>
    private void ResolveLanguageNames()
    {
        foreach (var entry in SoundBanks)
        {
            entry.Language = GetLanguageName(entry.LanguageId);
        }

        foreach (var entry in StreamingFiles)
        {
            entry.Language = GetLanguageName(entry.LanguageId);
        }

        foreach (var entry in ExternalFiles)
        {
            entry.Language = GetLanguageName(entry.LanguageId);
        }
    }

    private static void UpdateStartBlocks<TKey, TEntry>(FileLut<TKey, TEntry> lut, ref uint currentOffset)
        where TKey : struct, INumber<TKey>
        where TEntry : FileEntry<TKey>
    {
        foreach (var entry in lut)
        {
            // Align to block size if needed
            if (entry.BlockSize > 0 && currentOffset % entry.BlockSize != 0)
            {
                currentOffset += entry.BlockSize - currentOffset % entry.BlockSize;
            }

            entry.StartBlock = currentOffset;
            currentOffset += (uint) entry.Size;
        }
    }

    private static void WriteFileData<TKey, TEntry>(FileLut<TKey, TEntry> lut, BinaryWriter writer)
        where TKey : struct, INumber<TKey>
        where TEntry : FileEntry<TKey>
    {
        foreach (var entry in lut)
        {
            writer.BaseStream.Position = entry.StartBlock;
            writer.Write(entry.GetData());
        }
    }

    private void CompareLanguageMaps(PckFile other, List<string> differences)
    {
        if (Languages.Count != other.Languages.Count)
        {
            differences.Add($"Languages count mismatch: {Languages.Count} vs {other.Languages.Count}");
        }

        foreach (var (id, name) in Languages)
        {
            if (!other.Languages.TryGetValue(id, out var otherName))
            {
                differences.Add($"Language ID {id} ('{name}') missing in other package");
            }
            else if (name != otherName)
            {
                differences.Add($"Language ID {id} name mismatch: '{name}' vs '{otherName}'");
            }
        }

        foreach (var (id, name) in other.Languages)
        {
            if (!Languages.ContainsKey(id))
            {
                differences.Add($"Language ID {id} ('{name}') extra in other package");
            }
        }
    }

    private static void CompareLut<TKey, TEntry>(
        string lutName,
        FileLut<TKey, TEntry> lut,
        FileLut<TKey, TEntry> other,
        List<string> differences,
        List<string> warnings)
        where TKey : struct, INumber<TKey>
        where TEntry : FileEntry<TKey>
    {
        var entries = lut.Entries;
        var otherEntries = other.Entries;
        var minCount = Math.Min(entries.Count, otherEntries.Count);

        for (var i = 0; i < minCount; i++)
        {
            var entry = entries[i];
            var otherEntry = otherEntries[i];
            var prefix = $"{lutName}[{i}] (Id: {entry.Id:X})";

            if (!entry.Id.Equals(otherEntry.Id))
            {
                differences.Add($"{prefix}: Id mismatch: {entry.Id:X} vs {otherEntry.Id:X}");
            }

            if (entry.BlockSize != otherEntry.BlockSize)
            {
                differences.Add($"{prefix}: BlockSize mismatch: {entry.BlockSize} vs {otherEntry.BlockSize}");
            }

            if (entry.StartBlock != otherEntry.StartBlock)
            {
                warnings.Add($"{prefix}: StartBlock changed: {entry.StartBlock} -> {otherEntry.StartBlock}");
            }

            if (entry.LanguageId != otherEntry.LanguageId)
            {
                differences.Add($"{prefix}: LanguageId mismatch: {entry.LanguageId} vs {otherEntry.LanguageId}");
            }

            var data = entry.GetData();
            var otherData = otherEntry.GetData();

            if (data.Length != otherData.Length)
            {
                differences.Add($"{prefix}: Data length mismatch: {data.Length} vs {otherData.Length}");
            }
            else
            {
                var firstDiffIndex = -1;
                var diffCount = 0;

                for (var j = 0; j < data.Length; j++)
                {
                    if (data[j] != otherData[j])
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

            if (!entry.IsValid) differences.Add($"{prefix}: Original entry IsValid = false");
            if (!otherEntry.IsValid) differences.Add($"{prefix}: Reloaded entry IsValid = false");
        }

        for (var i = minCount; i < entries.Count; i++)
        {
            differences.Add($"{lutName}[{i}] (Id: {entries[i].Id:X}): Missing in reloaded package");
        }

        for (var i = minCount; i < otherEntries.Count; i++)
        {
            differences.Add($"{lutName}[{i}] (Id: {otherEntries[i].Id:X}): Extra in reloaded package");
        }
    }
}
