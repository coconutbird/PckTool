using System.CommandLine;
using System.Globalization;
using System.Text;

using Microsoft.Win32;

using PckTool.WWise;

namespace PckTool;

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
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand("Halo Wars Sound Unpacker - Extract and manipulate sound banks");

        // Global options
        var gameDirOption = new Option<string?>("--game-dir", "-g")
        {
            Description =
                "Path to the Halo Wars game directory. If not specified, attempts to find it automatically."
        };

        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "Output directory for extracted files.", DefaultValueFactory = _ => "dumps"
        };

        var verboseOption = new Option<bool>("--verbose", "-v") { Description = "Enable verbose output." };

        rootCommand.Options.Add(gameDirOption);
        rootCommand.Options.Add(outputOption);
        rootCommand.Options.Add(verboseOption);

        // Dump command - extracts all sound banks
        var dumpCommand = new Command("dump", "Extract all sound banks and WEM files from the game.");

        var soundBankOption = new Option<string?>("--soundbank", "-s")
        {
            Description = "Specific sound bank ID (hex) to extract. If not specified, extracts all sound banks."
        };

        var languageOption = new Option<string?>("--language", "-l")
        {
            Description =
                "Filter by language (e.g., 'English(US)', 'SFX'). If not specified, extracts all languages."
        };

        dumpCommand.Options.Add(soundBankOption);
        dumpCommand.Options.Add(languageOption);

        dumpCommand.SetAction(parseResult =>
        {
            var gameDir = parseResult.GetValue(gameDirOption);
            var output = parseResult.GetValue(outputOption) ?? "dumps";
            var verbose = parseResult.GetValue(verboseOption);
            var soundBank = parseResult.GetValue(soundBankOption);
            var language = parseResult.GetValue(languageOption);
            RunDump(gameDir, output, verbose, soundBank, language);
        });

        rootCommand.Subcommands.Add(dumpCommand);

        // Replace command - replaces a sound bank
        var replaceCommand = new Command("replace", "Replace a sound bank in the package file.");

        var replaceSoundBankOption = new Option<string>("--soundbank", "-s")
        {
            Description = "Sound bank ID (hex) to replace.", Required = true
        };

        var inputFileOption = new Option<string>("--input", "-i")
        {
            Description = "Path to the replacement .bnk file.", Required = true
        };

        replaceCommand.Options.Add(replaceSoundBankOption);
        replaceCommand.Options.Add(inputFileOption);

        replaceCommand.SetAction(parseResult =>
        {
            var gameDir = parseResult.GetValue(gameDirOption);
            var output = parseResult.GetValue(outputOption) ?? "dumps";
            var verbose = parseResult.GetValue(verboseOption);
            var soundBank = parseResult.GetValue(replaceSoundBankOption)!;
            var inputFile = parseResult.GetValue(inputFileOption)!;
            RunReplace(gameDir, output, verbose, soundBank, inputFile);
        });

        rootCommand.Subcommands.Add(replaceCommand);

        // List command - lists all sound banks
        var listCommand = new Command("list", "List all sound banks in the package file.");

        listCommand.SetAction(parseResult =>
        {
            var gameDir = parseResult.GetValue(gameDirOption);
            var verbose = parseResult.GetValue(verboseOption);
            RunList(gameDir, verbose);
        });

        rootCommand.Subcommands.Add(listCommand);

        // Info command - shows default paths and configuration
        var infoCommand = new Command("info", "Show default paths and configuration information.");

        infoCommand.SetAction(_ =>
        {
            RunInfo();
        });

        rootCommand.Subcommands.Add(infoCommand);

        // Default behavior (no command) - show help
        rootCommand.SetAction(_ =>
        {
            Log.Info("Use --help to see available commands and options.");
        });

        return rootCommand.Parse(args).Invoke();
    }

    private static void RunDump(
        string? gameDirArg,
        string outputDir,
        bool verbose,
        string? soundBankFilter,
        string? languageFilter)
    {
        var gameDir = gameDirArg ?? FindHaloWarsGameDirectory();

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

        var soundsPackagePath = GetSoundsPackagePath(gameDir);

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

        // Parse sound bank filter if provided
        uint? soundBankIdFilter = null;

        if (!string.IsNullOrWhiteSpace(soundBankFilter))
        {
            if (uint.TryParse(soundBankFilter, NumberStyles.HexNumber, null, out var parsedId))
            {
                soundBankIdFilter = parsedId;
                Log.Info("Filtering to sound bank: {0:X8}", parsedId);
            }
            else
            {
                Log.Error("Invalid sound bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)");

                return;
            }
        }

        Log.Info("Loading soundbanks...");

        foreach (var fileEntry in package.SoundBanksLut.Entries)
        {
            var languageId = fileEntry.LanguageId;
            var language = package.LanguageMap[languageId];

            // Apply language filter
            if (!string.IsNullOrWhiteSpace(languageFilter)
                && !language.Equals(languageFilter, StringComparison.OrdinalIgnoreCase))
            {
                if (verbose)
                {
                    Log.Info(
                        "Skipping soundbank {0:X8} (language {1} doesn't match filter)",
                        fileEntry.FileId,
                        language);
                }

                continue;
            }

            // Apply sound bank filter
            if (soundBankIdFilter.HasValue && fileEntry.FileId != soundBankIdFilter.Value)
            {
                if (verbose)
                {
                    Log.Info("Skipping soundbank {0:X8} (doesn't match filter)", fileEntry.FileId);
                }

                continue;
            }

            if (verbose)
            {
                Log.Info(
                    "Soundbank ID: {0:X8} Language: {1} Size: {2} bytes",
                    fileEntry.FileId,
                    language,
                    fileEntry.FileSize);
            }

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
                var path = Path.Join(outputDir, language);

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

    private static string GetSoundsPackagePath(string gameDir)
    {
        return Path.Join(
            gameDir,
            "sound",
            "wwise_2013",
            "GeneratedSoundBanks",
            "Windows",
            "Sounds.pck");
    }

    private static void EnsureDirectoryCreated(string path)
    {
        path = Path.GetDirectoryName(Path.GetFullPath(path))!;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void RunList(string? gameDirArg, bool verbose)
    {
        var gameDir = gameDirArg ?? FindHaloWarsGameDirectory();

        if (gameDir is null)
        {
            Log.Error("Failed to find Halo Wars game directory");

            return;
        }

        Log.Info("Found Halo Wars game directory: " + gameDir);

        var soundsPackagePath = GetSoundsPackagePath(gameDir);

        var package = new FilePackage(soundsPackagePath);

        if (!package.Load())
        {
            Log.Error("Failed to find sounds file");

            return;
        }

        Log.Info("Sound Banks in package:");
        Log.Info("------------------------");

        // Group by language for cleaner output
        var banksByLanguage = package.SoundBanksLut
                                     .Entries
                                     .GroupBy(e => package.LanguageMap[e.LanguageId])
                                     .OrderBy(g => g.Key);

        foreach (var languageGroup in banksByLanguage)
        {
            Log.Info("");
            Log.Info("Language: {0}", languageGroup.Key);

            foreach (var entry in languageGroup.OrderBy(e => e.FileId))
            {
                Log.Info("  {0:X8} - {1} bytes", entry.FileId, entry.FileSize);
            }
        }

        Log.Info("");
        Log.Info("Total: {0} sound banks", package.SoundBanksLut.Entries.Count);
    }

    private static void RunReplace(
        string? gameDirArg,
        string outputPath,
        bool verbose,
        string soundBankId,
        string inputFile)
    {
        var gameDir = gameDirArg ?? FindHaloWarsGameDirectory();

        if (gameDir is null)
        {
            Log.Error("Failed to find Halo Wars game directory");

            return;
        }

        Log.Info("Found Halo Wars game directory: " + gameDir);

        // Parse sound bank ID
        if (!uint.TryParse(soundBankId, NumberStyles.HexNumber, null, out var bankId))
        {
            Log.Error("Invalid sound bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)");

            return;
        }

        // Verify input file exists
        if (!File.Exists(inputFile))
        {
            Log.Error("Input file not found: {0}", inputFile);

            return;
        }

        var soundsPackagePath = GetSoundsPackagePath(gameDir);

        var package = new FilePackage(soundsPackagePath);

        if (!package.Load())
        {
            Log.Error("Failed to find sounds file");

            return;
        }

        // Find the sound bank entry to replace
        var entry = package.SoundBanksLut.Entries.FirstOrDefault(e => e.FileId == bankId);

        if (entry is null)
        {
            Log.Error("Sound bank {0:X8} not found in package", bankId);

            return;
        }

        Log.Info(
            "Found sound bank {0:X8} (Language: {1}, Size: {2} bytes)",
            bankId,
            package.LanguageMap[entry.LanguageId],
            entry.FileSize);

        // Read the replacement data
        var replacementData = File.ReadAllBytes(inputFile);
        Log.Info("Replacement file size: {0} bytes", replacementData.Length);

        // Replace the data
        entry.Data = replacementData;

        // Determine output path
        var outputFile = outputPath;

        if (Directory.Exists(outputPath))
        {
            outputFile = Path.Join(outputPath, "Sounds_modified.pck");
        }

        EnsureDirectoryCreated(outputFile);

        // Save the modified package
        Log.Info("Saving modified package to: {0}", outputFile);
        package.Save(outputFile);

        Log.Info("Done! Sound bank {0:X8} has been replaced.", bankId);
    }

    private static void RunInfo()
    {
        Log.Info("=== SoundsUnpack Configuration Info ===");
        Log.Info("");

        // Game directory
        var gameDir = FindHaloWarsGameDirectory();

        if (gameDir is not null)
        {
            Log.Info("Default Game Directory: {0}", gameDir);
        }
        else
        {
            Log.Warn("Default Game Directory: Not found (use --game-dir to specify)");
        }

        Log.Info("");

        // Sound table
        if (gameDir is not null)
        {
            var soundTablePath = FindSoundTableXml(gameDir);

            if (soundTablePath is not null)
            {
                Log.Info("Sound Table File: {0}", soundTablePath);
            }
            else
            {
                Log.Warn("Sound Table File: Not found in game directory");
            }

            // Sounds.pck path
            var soundsPackagePath = GetSoundsPackagePath(gameDir);

            if (File.Exists(soundsPackagePath))
            {
                Log.Info("Sounds Package: {0}", soundsPackagePath);
                var fileInfo = new FileInfo(soundsPackagePath);
                Log.Info("  Size: {0:N0} bytes ({1:N2} MB)", fileInfo.Length, fileInfo.Length / 1024.0 / 1024.0);
            }
            else
            {
                Log.Warn("Sounds Package: Not found at expected path");
                Log.Warn("  Expected: {0}", soundsPackagePath);
            }
        }

        Log.Info("");
        Log.Info("Default Output Directory: dumps");
    }
}
