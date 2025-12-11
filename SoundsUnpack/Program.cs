using System.Text;

using Microsoft.Win32;

using SoundsUnpack.WWise;

namespace SoundsUnpack;

public static class BinaryReaderExtensions
{
    public static string ReadWString(this BinaryReader reader)
    {
        var builder = new StringBuilder();

        while (true)
        {
            var buffer = reader.ReadUInt16();

            if (buffer == 0)
            {
                return builder.ToString();
            }

            builder.Append((char) buffer);
        }
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var gameDir = FindHaloWarsGameDirectory();

        if (gameDir is null)
        {
            Log.Error("Failed to find Halo Wars game directory");

            return;
        }
        
        Log.Info("Found Halo Wars game directory: " + gameDir);
        
        var soundTablePath = FindSoundTableXml(gameDir);

        if (soundTablePath is null)
        {
            Log.Warn("Failed to find sound table, cue names will not be resolved!");
        }

        var soundsPackagePath = Path.Join(
            gameDir,
            "sound",
            "wwise_2013",
            "GeneratedSoundBanks",
            "Windows",
            "Sounds.pck");

        var package = new FilePackage(soundsPackagePath);

        if (!package.Load())
        {
            Log.Error("Failed to find sounds file");

            return;
        }

        // Phase 1: Load all soundbanks, grouped by language for cross-bank reference support
        // Key: LanguageId -> (SoundbankId -> SoundBank)
        var soundbanksByLanguage = new Dictionary<uint, Dictionary<uint, SoundBank>>();
        var failed = 1;

        Log.Info("Loading soundbanks...");

        foreach (var fileEntry in package.SoundBanksLut.Entries)
        {
            var languageId = fileEntry.LanguageId;
            var language = package.LanguageMap[languageId];

            Log.Info(
                "Soundbank ID: {0:X8} Language: {1} Size: {2} bytes",
                fileEntry.FileId,
                language,
                fileEntry.FileSize);

            var soundbank = new SoundBank();

            if (!soundbank.Read(new BinaryReader(new MemoryStream(fileEntry.Data))))
            {
                Log.Error("  Failed to parse soundbank: " + failed++);

                // well we failed to parse the whole thing, but we can still extract the wems
                if (!soundbank.IsMediaLoaded)
                {
                    continue;
                }
            }

            var bankId = soundbank.SoundbankId;

            if (bankId is null)
            {
                Log.Warn("  Soundbank has no ID, skipping");

                continue;
            }

            if (!soundbank.IsValid)
            {
                Log.Warn("  Soundbank is not valid: {0}", bankId);
            }

            // Get or create the language group
            if (!soundbanksByLanguage.TryGetValue(languageId, out var languageBanks))
            {
                languageBanks = new Dictionary<uint, SoundBank>();
                soundbanksByLanguage[languageId] = languageBanks;
            }

            languageBanks[bankId.Value] = soundbank;
        }

        // Phase 2: Load sound table and resolve all file IDs with cross-bank support
        var soundTable = new SoundTable();

        if (!string.IsNullOrWhiteSpace(soundTablePath) && !soundTable.Load(soundTablePath))
        {
            Log.Error("Failed to load sound table, cue names will not be resolved!");

            return;
        }

        Log.Info("Resolving cue names...");

        // Build a global lookup for cross-language bank references (e.g., SFX banks)
        // This allows soundbanks to reference banks from any language
        var globalBankLookup = new Dictionary<uint, SoundBank>();

        foreach (var languageBanks in soundbanksByLanguage.Values)
        {
            foreach (var (bankId, soundbank) in languageBanks)
            {
                // If multiple languages have the same bank ID, prefer the first one encountered
                // (typically language-neutral banks like SFX will only exist once)
                globalBankLookup.TryAdd(bankId, soundbank);
            }
        }

        // Resolve for each language group, with fallback to global lookup for cross-language refs
        foreach (var (languageId, languageBanks) in soundbanksByLanguage)
        {
            var language = package.LanguageMap[languageId];
            Log.Info("  Resolving for language: {0} ({1} banks)", language, languageBanks.Count);

            // Create a lookup function that:
            // 1. First tries to find the bank in the current language (preferred)
            // 2. Falls back to global lookup for cross-language references (e.g., SFX banks)
            SoundBank? BankLookup(uint bankId)
            {
                // Prefer same-language bank if it exists
                if (languageBanks.TryGetValue(bankId, out var sameLanguageBank))
                {
                    return sameLanguageBank;
                }

                // Fall back to global lookup for cross-language references
                return globalBankLookup.GetValueOrDefault(bankId);
            }

            // Resolve each bank in this language group with access to all banks
            foreach (var soundbank in languageBanks.Values)
            {
                soundTable.ResolveFileIds(soundbank, BankLookup);
            }
        }

        // Phase 3: Extract WEM files with resolved cue names
        Log.Info("Extracting WEM files...");

        foreach (var (languageId, languageBanks) in soundbanksByLanguage)
        {
            var language = package.LanguageMap[languageId];

            foreach (var (soundbankId, soundbank) in languageBanks)
            {
                var path = Path.Join("dumps", language);

                EnsureDirectoryCreated(path);

                var bnkFile = Path.Join(path, $"{soundbankId:X8}.bnk");

                EnsureDirectoryCreated(bnkFile);

                // Track how many times each cue name is used for unique filenames
                var usedFiles = new Dictionary<string, int>();

                foreach (var wem in soundbank.DataChunk?.Data ?? [])
                {
                    if (!wem.IsValid)
                    {
                        Log.Warn("  Invalid WEM data!");

                        continue;
                    }

                    var cueName = soundTable.GetCueNameByFileId(wem.Id);
                    var wemFileName = $"{wem.Id}";

                    if (cueName is not null)
                    {
                        wemFileName += $"_{cueName}";

                        var count = usedFiles.GetValueOrDefault(cueName, 0);

                        wemFileName += $"_{count}";
                        usedFiles[cueName] = count + 1;
                    }
                    else
                    {
                        Log.Warn("  No cue name found for {0:X8} ({1})", soundbankId, wemFileName);
                    }

                    var wemFile = Path.Join(path, $"{soundbankId:X8}", $"{wemFileName}.wem");

                    EnsureDirectoryCreated(wemFile);

                    File.WriteAllBytes(wemFile, wem.Data);
                }

                // Find the original file entry to write the .bnk file
                var fileEntry =
                    package.SoundBanksLut.Entries.First(e => e.FileId == soundbankId && e.LanguageId == languageId);

                File.WriteAllBytes(bnkFile, fileEntry.Data);
            }
        }

        Log.Info("Done!");
    }

    private static string? FindHaloWarsGameDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 459220");

                if (key?.GetValue("InstallLocation") is string installPath && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    private static string? FindSoundTableXml(string gameDir)
    {
        return Directory.GetFiles(gameDir, "soundtable.xml", SearchOption.AllDirectories).FirstOrDefault();
    }

    private static void EnsureDirectoryCreated(string path)
    {
        path = Path.GetDirectoryName(Path.GetFullPath(path))!;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
