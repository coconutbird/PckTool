using System.Numerics;

using PckTool.Core.WWise.Bnk;
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
    ///     Creates a new empty package file.
    /// </summary>
    public static PckFile Create()
    {
        return new PckFile();
    }

    /// <summary>
    ///     Adds a language mapping to this package.
    /// </summary>
    /// <param name="languageId">The language ID.</param>
    /// <param name="languageName">The language name.</param>
    public void AddLanguage(uint languageId, string languageName)
    {
        Languages[languageId] = languageName;
    }

    /// <summary>
    ///     Adds a sound bank entry from raw byte data.
    /// </summary>
    /// <param name="id">The sound bank ID.</param>
    /// <param name="data">The BNK file data.</param>
    /// <param name="languageId">The language ID (default: 0).</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name for the sound bank.</param>
    public SoundBankEntry AddSoundBank(
        uint id,
        byte[] data,
        uint languageId = 0,
        uint blockSize = 16,
        string? name = null)
    {
        var entry = new SoundBankEntry
        {
            Id = id,
            LanguageId = languageId,
            BlockSize = blockSize,
            Name = name,
            Language = GetLanguageName(languageId)
        };

        entry.SetOriginalData(data);

        SoundBanks.Add(entry);

        return entry;
    }

    /// <summary>
    ///     Adds a sound bank entry from a BNK file.
    /// </summary>
    /// <param name="id">The sound bank ID.</param>
    /// <param name="bnkFilePath">Path to the BNK file.</param>
    /// <param name="languageId">The language ID (default: 0).</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name for the sound bank.</param>
    public SoundBankEntry AddSoundBank(
        uint id,
        string bnkFilePath,
        uint languageId = 0,
        uint blockSize = 16,
        string? name = null)
    {
        var data = File.ReadAllBytes(bnkFilePath);

        return AddSoundBank(id, data, languageId, blockSize, name);
    }

    /// <summary>
    ///     Adds a sound bank entry from a SoundBank object.
    /// </summary>
    /// <param name="soundBank">The SoundBank object to add.</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name for the sound bank.</param>
    public SoundBankEntry AddSoundBank(SoundBank soundBank, uint blockSize = 16, string? name = null)
    {
        var data = soundBank.ToByteArray();

        return AddSoundBank(soundBank.Id, data, soundBank.LanguageId, blockSize, name);
    }

    /// <summary>
    ///     Adds a streaming file entry from raw byte data.
    /// </summary>
    /// <param name="id">The file ID.</param>
    /// <param name="data">The WEM file data.</param>
    /// <param name="languageId">The language ID (default: 0).</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name.</param>
    public StreamingFileEntry AddStreamingFile(
        uint id,
        byte[] data,
        uint languageId = 0,
        uint blockSize = 16,
        string? name = null)
    {
        var entry = new StreamingFileEntry
        {
            Id = id,
            LanguageId = languageId,
            BlockSize = blockSize,
            Name = name,
            Language = GetLanguageName(languageId)
        };

        entry.SetOriginalData(data);

        StreamingFiles.Add(entry);

        return entry;
    }

    /// <summary>
    ///     Adds a streaming file entry from a WEM file.
    /// </summary>
    /// <param name="id">The file ID.</param>
    /// <param name="wemFilePath">Path to the WEM file.</param>
    /// <param name="languageId">The language ID (default: 0).</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name.</param>
    public StreamingFileEntry AddStreamingFile(
        uint id,
        string wemFilePath,
        uint languageId = 0,
        uint blockSize = 16,
        string? name = null)
    {
        var data = File.ReadAllBytes(wemFilePath);

        return AddStreamingFile(id, data, languageId, blockSize, name);
    }

    /// <summary>
    ///     Adds an external file entry from raw byte data.
    /// </summary>
    /// <param name="id">The file ID (64-bit).</param>
    /// <param name="data">The file data.</param>
    /// <param name="languageId">The language ID (default: 0).</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name.</param>
    public ExternalFileEntry AddExternalFile(
        ulong id,
        byte[] data,
        uint languageId = 0,
        uint blockSize = 16,
        string? name = null)
    {
        var entry = new ExternalFileEntry
        {
            Id = id,
            LanguageId = languageId,
            BlockSize = blockSize,
            Name = name,
            Language = GetLanguageName(languageId)
        };

        entry.SetOriginalData(data);

        ExternalFiles.Add(entry);

        return entry;
    }

    /// <summary>
    ///     Adds an external file entry from a file path.
    /// </summary>
    /// <param name="id">The file ID (64-bit).</param>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="languageId">The language ID (default: 0).</param>
    /// <param name="blockSize">The block size alignment (default: 16).</param>
    /// <param name="name">Optional human-readable name.</param>
    public ExternalFileEntry AddExternalFile(
        ulong id,
        string filePath,
        uint languageId = 0,
        uint blockSize = 16,
        string? name = null)
    {
        var data = File.ReadAllBytes(filePath);

        return AddExternalFile(id, data, languageId, blockSize, name);
    }

    /// <summary>
    ///     Gets the language name for a language ID.
    /// </summary>
    public string? GetLanguageName(uint languageId)
    {
        return Languages.GetValueOrDefault(languageId);
    }

    /// <summary>
    ///     Replaces a sound bank entry with data from a file.
    /// </summary>
    /// <param name="id">The sound bank ID to replace.</param>
    /// <param name="bnkFilePath">Path to the replacement BNK file.</param>
    /// <returns>True if the entry was found and replaced, false if not found.</returns>
    public bool ReplaceSoundBank(uint id, string bnkFilePath)
    {
        var entry = SoundBanks[id];

        if (entry is null)
        {
            return false;
        }

        entry.ReplaceWith(bnkFilePath);

        return true;
    }

    /// <summary>
    ///     Replaces a sound bank entry with raw byte data.
    /// </summary>
    /// <param name="id">The sound bank ID to replace.</param>
    /// <param name="data">The replacement BNK data.</param>
    /// <returns>True if the entry was found and replaced, false if not found.</returns>
    public bool ReplaceSoundBank(uint id, byte[] data)
    {
        var entry = SoundBanks[id];

        if (entry is null)
        {
            return false;
        }

        entry.ReplaceWith(data);

        return true;
    }

    /// <summary>
    ///     Replaces a sound bank entry by name with data from a file.
    /// </summary>
    /// <param name="name">The sound bank name to find.</param>
    /// <param name="bnkFilePath">Path to the replacement BNK file.</param>
    /// <param name="languageId">Optional language ID to match. If null, replaces all language variants.</param>
    /// <returns>The number of entries replaced.</returns>
    public int ReplaceSoundBankByName(string name, string bnkFilePath, uint? languageId = null)
    {
        var count = 0;

        foreach (var entry in SoundBanks)
        {
            if (entry.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
            {
                if (languageId is null || entry.LanguageId == languageId)
                {
                    entry.ReplaceWith(bnkFilePath);
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    ///     Replaces a sound bank entry by name with raw byte data.
    /// </summary>
    /// <param name="name">The sound bank name to find.</param>
    /// <param name="data">The replacement BNK data.</param>
    /// <param name="languageId">Optional language ID to match. If null, replaces all language variants.</param>
    /// <returns>The number of entries replaced.</returns>
    public int ReplaceSoundBankByName(string name, byte[] data, uint? languageId = null)
    {
        var count = 0;

        foreach (var entry in SoundBanks)
        {
            if (entry.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
            {
                if (languageId is null || entry.LanguageId == languageId)
                {
                    entry.ReplaceWith(data);
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    ///     Gets a sound bank entry by ID.
    /// </summary>
    /// <param name="id">The sound bank ID.</param>
    /// <returns>The entry, or null if not found.</returns>
    public SoundBankEntry? GetSoundBank(uint id)
    {
        return SoundBanks[id];
    }

    /// <summary>
    ///     Gets all sound bank entries matching a name.
    /// </summary>
    /// <param name="name">The sound bank name to find.</param>
    /// <param name="languageId">Optional language ID to filter by.</param>
    /// <returns>All matching entries.</returns>
    public IEnumerable<SoundBankEntry> GetSoundBanksByName(string name, uint? languageId = null)
    {
        foreach (var entry in SoundBanks)
        {
            if (entry.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
            {
                if (languageId is null || entry.LanguageId == languageId)
                {
                    yield return entry;
                }
            }
        }
    }

#region WEM Replacement

    /// <summary>
    ///     Replaces a WEM file by ID, checking both embedded (in soundbanks) and streaming locations.
    /// </summary>
    /// <param name="id">The WEM source ID to replace.</param>
    /// <param name="data">The replacement WEM data.</param>
    /// <param name="updateHircSizes">If true, updates InMemoryMediaSize in all HIRC references.</param>
    /// <returns>A result indicating where the WEM was found and how many references were updated.</returns>
    public WemReplacementResult ReplaceWem(uint id, byte[] data, bool updateHircSizes = true)
    {
        var result = new WemReplacementResult { SourceId = id };

        // Check streaming files first (more common for large audio)
        var streamingEntry = StreamingFiles[id];

        if (streamingEntry is not null)
        {
            streamingEntry.ReplaceWith(data);
            result.ReplacedInStreaming = true;
        }

        // Check embedded media in all soundbanks
        foreach (var bankEntry in SoundBanks)
        {
            var bank = bankEntry.Parse();

            if (bank is null || !bank.Media.Contains(id))
            {
                continue;
            }

            // Replace embedded media and update HIRC
            var updated = bank.ReplaceWem(id, data, updateHircSizes);
            result.EmbeddedBanksModified++;
            result.HircReferencesUpdated += updated;

            // Re-serialize the modified bank
            bankEntry.ReplaceWith(bank.ToByteArray());
        }

        // If only in streaming (not embedded), still update HIRC sizes across banks
        if (result.ReplacedInStreaming && result.EmbeddedBanksModified == 0 && updateHircSizes)
        {
            result.HircReferencesUpdated = UpdateHircMediaSizes(id, (uint) data.Length);
        }

        if (!result.ReplacedInStreaming && result.EmbeddedBanksModified == 0)
        {
            throw new KeyNotFoundException($"WEM with ID 0x{id:X8} not found in streaming files or any soundbank.");
        }

        return result;
    }

    /// <summary>
    ///     Replaces a WEM file by ID from a file path.
    /// </summary>
    /// <param name="id">The WEM source ID to replace.</param>
    /// <param name="filePath">Path to the replacement WEM file.</param>
    /// <param name="updateHircSizes">If true, updates InMemoryMediaSize in all HIRC references.</param>
    /// <returns>A result indicating where the WEM was found and how many references were updated.</returns>
    public WemReplacementResult ReplaceWemFromFile(uint id, string filePath, bool updateHircSizes = true)
    {
        var data = File.ReadAllBytes(filePath);

        return ReplaceWem(id, data, updateHircSizes);
    }

    /// <summary>
    ///     Replaces a streaming WEM file by ID.
    /// </summary>
    /// <param name="id">The streaming file ID to replace.</param>
    /// <param name="data">The replacement WEM data.</param>
    /// <param name="updateHircSizes">If true, updates InMemoryMediaSize in all soundbanks that reference this WEM.</param>
    /// <returns>The number of HIRC references updated (0 if updateHircSizes is false or no references found).</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the streaming file ID doesn't exist.</exception>
    public int ReplaceStreamingWem(uint id, byte[] data, bool updateHircSizes = true)
    {
        var entry = StreamingFiles[id];

        if (entry is null)
        {
            throw new KeyNotFoundException($"Streaming WEM with ID 0x{id:X8} not found.");
        }

        entry.ReplaceWith(data);

        if (!updateHircSizes)
        {
            return 0;
        }

        return UpdateHircMediaSizes(id, (uint) data.Length);
    }

    /// <summary>
    ///     Replaces a streaming WEM file by ID from a file path.
    /// </summary>
    /// <param name="id">The streaming file ID to replace.</param>
    /// <param name="filePath">Path to the replacement WEM file.</param>
    /// <param name="updateHircSizes">If true, updates InMemoryMediaSize in all soundbanks that reference this WEM.</param>
    /// <returns>The number of HIRC references updated.</returns>
    public int ReplaceStreamingWemFromFile(uint id, string filePath, bool updateHircSizes = true)
    {
        var data = File.ReadAllBytes(filePath);

        return ReplaceStreamingWem(id, data, updateHircSizes);
    }

    /// <summary>
    ///     Replaces an external file by ID.
    /// </summary>
    /// <param name="id">The external file ID (64-bit) to replace.</param>
    /// <param name="data">The replacement file data.</param>
    /// <returns>True if the entry was found and replaced.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the external file ID doesn't exist.</exception>
    public bool ReplaceExternalFile(ulong id, byte[] data)
    {
        var entry = ExternalFiles[id];

        if (entry is null)
        {
            throw new KeyNotFoundException($"External file with ID 0x{id:X16} not found.");
        }

        entry.ReplaceWith(data);

        return true;
    }

    /// <summary>
    ///     Replaces an external file by ID from a file path.
    /// </summary>
    /// <param name="id">The external file ID (64-bit) to replace.</param>
    /// <param name="filePath">Path to the replacement file.</param>
    /// <returns>True if the entry was found and replaced.</returns>
    public bool ReplaceExternalFileFromFile(ulong id, string filePath)
    {
        var data = File.ReadAllBytes(filePath);

        return ReplaceExternalFile(id, data);
    }

    /// <summary>
    ///     Updates InMemoryMediaSize in all soundbanks for a given source ID.
    ///     This parses each soundbank, updates the HIRC references, and replaces the soundbank data.
    /// </summary>
    /// <param name="sourceId">The WEM source ID.</param>
    /// <param name="newSize">The new size to set.</param>
    /// <returns>The total number of HIRC references updated across all soundbanks.</returns>
    public int UpdateHircMediaSizes(uint sourceId, uint newSize)
    {
        var totalUpdated = 0;

        foreach (var entry in SoundBanks)
        {
            var soundBank = entry.Parse();

            if (soundBank is null)
            {
                continue;
            }

            var updated = soundBank.UpdateMediaSize(sourceId, newSize);

            if (updated > 0)
            {
                // Re-serialize the modified soundbank
                entry.ReplaceWith(soundBank.ToByteArray());
                totalUpdated += updated;
            }
        }

        return totalUpdated;
    }

    /// <summary>
    ///     Gets a streaming file entry by ID.
    /// </summary>
    /// <param name="id">The streaming file ID.</param>
    /// <returns>The entry, or null if not found.</returns>
    public StreamingFileEntry? GetStreamingFile(uint id)
    {
        return StreamingFiles[id];
    }

    /// <summary>
    ///     Gets an external file entry by ID.
    /// </summary>
    /// <param name="id">The external file ID (64-bit).</param>
    /// <returns>The entry, or null if not found.</returns>
    public ExternalFileEntry? GetExternalFile(ulong id)
    {
        return ExternalFiles[id];
    }

#endregion

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
