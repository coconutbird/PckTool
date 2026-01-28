using System.CommandLine;
using System.Globalization;

using Microsoft.Win32;

using PckTool.Core.HaloWars;
using PckTool.Core.Package;
using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Pck;

namespace PckTool;

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

        // Browse command - browse banks in a package using PackageBrowser
        var browseCommand = new Command("browse", "Browse sound banks in the package file.");

        var browseBankOption = new Option<string?>("--bank", "-b")
        {
            Description = "Specific bank ID (hex) to show details for."
        };

        browseCommand.Options.Add(languageOption);
        browseCommand.Options.Add(browseBankOption);

        browseCommand.SetAction(parseResult =>
        {
            var gameDir = parseResult.GetValue(gameDirOption);
            var verbose = parseResult.GetValue(verboseOption);
            var language = parseResult.GetValue(languageOption);
            var bankId = parseResult.GetValue(browseBankOption);
            RunBrowse(gameDir, verbose, language, bankId);
        });

        rootCommand.Subcommands.Add(browseCommand);

        // Sounds command - list sounds in a specific bank
        var soundsCommand = new Command("sounds", "List all sounds in a specific bank.");

        var soundsBankOption = new Option<string>("--bank", "-b")
        {
            Description = "Bank ID (hex) to list sounds from.", Required = true
        };

        soundsCommand.Options.Add(soundsBankOption);

        soundsCommand.SetAction(parseResult =>
        {
            var gameDir = parseResult.GetValue(gameDirOption);
            var verbose = parseResult.GetValue(verboseOption);
            var bankId = parseResult.GetValue(soundsBankOption)!;
            RunSounds(gameDir, verbose, bankId);
        });

        rootCommand.Subcommands.Add(soundsCommand);

        // Project command - manage project files
        var projectCommand = new Command("project", "Manage project files.");

        // Project create subcommand
        var projectCreateCommand = new Command("create", "Create a new project file.");

        var projectNameOption = new Option<string>("--name", "-n")
        {
            Description = "Project name.", DefaultValueFactory = _ => "Untitled Project"
        };

        var projectFileOption = new Option<string>("--file", "-f")
        {
            Description = "Path to save the project file.", Required = true
        };

        projectCreateCommand.Options.Add(projectNameOption);
        projectCreateCommand.Options.Add(projectFileOption);
        projectCreateCommand.Options.Add(gameDirOption);

        projectCreateCommand.SetAction(parseResult =>
        {
            var name = parseResult.GetValue(projectNameOption) ?? "Untitled Project";
            var file = parseResult.GetValue(projectFileOption)!;
            var gameDir = parseResult.GetValue(gameDirOption);
            RunProjectCreate(name, file, gameDir);
        });

        projectCommand.Subcommands.Add(projectCreateCommand);

        // Project info subcommand
        var projectInfoCommand = new Command("info", "Show project information.");

        var projectPathOption = new Option<string>("--project", "-p")
        {
            Description = "Path to the project file.", Required = true
        };

        projectInfoCommand.Options.Add(projectPathOption);

        projectInfoCommand.SetAction(parseResult =>
        {
            var projectPath = parseResult.GetValue(projectPathOption)!;
            RunProjectInfo(projectPath);
        });

        projectCommand.Subcommands.Add(projectInfoCommand);

        rootCommand.Subcommands.Add(projectCommand);

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

        var package = PckFile.Load(soundsPackagePath);

        if (package is null)
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

        foreach (var fileEntry in package.SoundBanks)
        {
            var languageId = fileEntry.LanguageId;
            var language = package.Languages[languageId];

            // Apply language filter
            if (!string.IsNullOrWhiteSpace(languageFilter)
                && !language.Equals(languageFilter, StringComparison.OrdinalIgnoreCase))
            {
                if (verbose)
                {
                    Log.Info(
                        "Skipping soundbank {0:X8} (language {1} doesn't match filter)",
                        fileEntry.Id,
                        language);
                }

                continue;
            }

            // Apply sound bank filter
            if (soundBankIdFilter.HasValue && fileEntry.Id != soundBankIdFilter.Value)
            {
                if (verbose)
                {
                    Log.Info("Skipping soundbank {0:X8} (doesn't match filter)", fileEntry.Id);
                }

                continue;
            }

            if (verbose)
            {
                Log.Info(
                    "Soundbank ID: {0:X8} Language: {1} Size: {2} bytes",
                    fileEntry.Id,
                    language,
                    fileEntry.Size);
            }

            var soundbank = SoundBank.Parse(fileEntry.GetData());

            if (soundbank is null)
            {
                Log.Error("  Failed to parse soundbank: " + failed++);

                continue;
            }

            // Check if we have media even if parsing partially failed
            if (soundbank.Media.Count == 0 && !soundbank.IsValid)
            {
                Log.Warn("  Soundbank has no media and is not valid, skipping");

                continue;
            }

            var bankId = soundbank.Id;

            if (!soundbank.IsValid)
            {
                Log.Warn("  Soundbank is not valid: {0:X8}", bankId);
            }

            // Get or create the language group
            if (!soundbanksByLanguage.TryGetValue(languageId, out var languageBanks))
            {
                languageBanks = new Dictionary<uint, SoundBank>();
                soundbanksByLanguage[languageId] = languageBanks;
            }

            languageBanks[bankId] = soundbank;
        }

        // Phase 2: Load sound table and resolve all file IDs with cross-bank support
        var soundTable = new SoundTable();

        if (!string.IsNullOrWhiteSpace(soundTablePath) && !soundTable.Load(soundTablePath))
        {
            Log.Error("Failed to load sound table, cue names will not be resolved!");

            return;
        }

        Log.Info("Loaded {0} cue entries from sound table", soundTable.Cues.Count);
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
            var language = package.Languages[languageId];
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

        Log.Info("Resolved {0} file IDs to cue references", soundTable.ResolvedFileIdCount);

        // Phase 3: Extract WEM files with resolved cue names and generate metadata
        Log.Info("Extracting WEM files...");

        foreach (var (languageId, languageBanks) in soundbanksByLanguage)
        {
            var language = package.Languages[languageId];

            foreach (var (soundbankId, soundbank) in languageBanks)
            {
                var bankDir = Path.Join(outputDir, language, $"{soundbankId:X8}");

                EnsureDirectoryCreated(bankDir + Path.DirectorySeparatorChar);

                // Create metadata for this soundbank
                var metadata = new WemMetadata
                {
                    SoundbankId = soundbankId, Language = language, LanguageId = languageId
                };

                foreach (var (wemId, wemData) in soundbank.Media)
                {
                    // Use simple numeric filename
                    var wemFile = Path.Join(bankDir, $"{wemId}.wem");

                    File.WriteAllBytes(wemFile, wemData);

                    // Get all cue references for this WEM file (many-to-many relationship with cross-bank support)
                    var cueRefs = soundTable.GetCueReferencesByFileId(wemId);

                    if (cueRefs.Count == 0)
                    {
                        Log.Warn("  No cue name found for WEM {0} in soundbank {1:X8}", wemId, soundbankId);
                    }

                    // Convert to metadata format with full cue information
                    var cueMetadataList = cueRefs
                                          .OrderBy(r => r.CueName)
                                          .ThenBy(r => r.SourceBankId)
                                          .Select(r => new CueMetadata
                                          {
                                              Name = r.CueName,
                                              EventId = r.CueIndex,
                                              SourceBankId = r.SourceBankId
                                          })
                                          .ToList();

                    // Add to metadata
                    metadata.Files.Add(new WemFileEntry { Id = wemId, Size = wemData.Length, Cues = cueMetadataList });
                }

                // Write metadata file for this soundbank
                var metadataFile = Path.Join(bankDir, "metadata.json");
                metadata.Save(metadataFile);

                // Also write the .bnk file to the language directory
                var bnkFile = Path.Join(outputDir, language, $"{soundbankId:X8}.bnk");
                var fileEntry = package.SoundBanks.First(e => e.Id == soundbankId && e.LanguageId == languageId);

                File.WriteAllBytes(bnkFile, fileEntry.GetData());
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

        var package = PckFile.Load(soundsPackagePath);

        if (package is null)
        {
            Log.Error("Failed to find sounds file");

            return;
        }

        Log.Info("Sound Banks in package:");
        Log.Info("------------------------");

        // Group by language for cleaner output
        var banksByLanguage = package.SoundBanks
                                     .GroupBy(e => package.Languages[e.LanguageId])
                                     .OrderBy(g => g.Key);

        foreach (var languageGroup in banksByLanguage)
        {
            Log.Info("");
            Log.Info("Language: {0}", languageGroup.Key);

            foreach (var entry in languageGroup.OrderBy(e => e.Id))
            {
                Log.Info("  {0:X8} - {1} bytes", entry.Id, entry.Size);
            }
        }

        Log.Info("");
        Log.Info("Total: {0} sound banks", package.SoundBanks.Count());
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

        var package = PckFile.Load(soundsPackagePath);

        if (package is null)
        {
            Log.Error("Failed to find sounds file");

            return;
        }

        // Find the sound bank entry to replace
        var entry = package.SoundBanks[bankId];

        if (entry is null)
        {
            Log.Error("Sound bank {0:X8} not found in package", bankId);

            return;
        }

        Log.Info(
            "Found sound bank {0:X8} (Language: {1}, Size: {2} bytes)",
            bankId,
            package.Languages[entry.LanguageId],
            entry.Size);

        // Read the replacement data
        var replacementData = File.ReadAllBytes(inputFile);
        Log.Info("Replacement file size: {0} bytes", replacementData.Length);

        // Replace the data
        entry.ReplaceWith(replacementData);

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

    private static void RunBrowse(string? gameDirArg, bool verbose, string? languageFilter, string? bankIdFilter)
    {
        var gameDir = gameDirArg ?? FindHaloWarsGameDirectory();

        if (gameDir is null)
        {
            Log.Error("Failed to find Halo Wars game directory");

            return;
        }

        Log.Info("Found Halo Wars game directory: " + gameDir);

        using var browser = new PackageBrowser();

        var soundsPackagePath = GetSoundsPackagePath(gameDir);

        if (!browser.LoadPackage(soundsPackagePath))
        {
            Log.Error("Failed to load sounds package");

            return;
        }

        // Try to load sound table for cue names
        var soundTablePath = FindSoundTableXml(gameDir);

        if (soundTablePath is not null)
        {
            if (browser.LoadSoundTable(soundTablePath))
            {
                Log.Info("Sound table loaded");
            }
        }

        // Parse bank ID filter if provided
        uint? bankId = null;

        if (!string.IsNullOrWhiteSpace(bankIdFilter))
        {
            if (uint.TryParse(bankIdFilter, NumberStyles.HexNumber, null, out var parsedId))
            {
                bankId = parsedId;
            }
            else
            {
                Log.Error("Invalid bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)");

                return;
            }
        }

        // If a specific bank is requested, show details
        if (bankId.HasValue)
        {
            var details = browser.GetBankDetails(bankId.Value);

            if (details is null)
            {
                Log.Error("Bank {0:X8} not found", bankId.Value);

                return;
            }

            Log.Info("");
            Log.Info("=== Bank Details: {0} ===", details.IdHex);
            Log.Info("Language:     {0}", details.Language);
            Log.Info("Size:         {0}", details.SizeFormatted);
            Log.Info("Version:      0x{0:X}", details.Version);
            Log.Info("Valid:        {0}", details.IsValid);
            Log.Info("Sounds:       {0}", details.SoundCount);
            Log.Info("Events:       {0}", details.EventCount);
            Log.Info("Actions:      {0}", details.ActionCount);
            Log.Info("Media Files:  {0}", details.MediaCount);

            if (details.Sounds.Count > 0)
            {
                Log.Info("");
                Log.Info("Sounds:");

                foreach (var sound in details.Sounds)
                {
                    var embedded = sound.HasEmbeddedMedia ? "[embedded]" : "[streaming]";
                    Log.Info("  {0} - {1} {2}", sound.SourceIdHex, sound.DisplayName, embedded);
                }
            }

            return;
        }

        // Otherwise, list all banks
        Log.Info("");
        Log.Info("Sound Banks:");
        Log.Info("------------");

        // Parse language filter
        uint? languageId = null;

        if (!string.IsNullOrWhiteSpace(languageFilter))
        {
            foreach (var (id, name) in browser.Languages)
            {
                if (name.Equals(languageFilter, StringComparison.OrdinalIgnoreCase))
                {
                    languageId = id;

                    break;
                }
            }

            if (!languageId.HasValue)
            {
                Log.Warn("Language '{0}' not found, showing all languages", languageFilter);
            }
        }

        var banks = browser.GetBanks(languageId).ToList();
        var banksByLanguage = banks.GroupBy(b => b.Language).OrderBy(g => g.Key);

        foreach (var group in banksByLanguage)
        {
            Log.Info("");
            Log.Info("Language: {0}", group.Key);

            foreach (var bank in group.OrderBy(b => b.Id))
            {
                Log.Info("  {0} - {1} ({2} sounds)", bank.IdHex, bank.SizeFormatted, bank.SoundCount);
            }
        }

        Log.Info("");
        Log.Info("Total: {0} banks", banks.Count);
    }

    private static void RunSounds(string? gameDirArg, bool verbose, string bankIdArg)
    {
        var gameDir = gameDirArg ?? FindHaloWarsGameDirectory();

        if (gameDir is null)
        {
            Log.Error("Failed to find Halo Wars game directory");

            return;
        }

        // Parse bank ID
        if (!uint.TryParse(bankIdArg, NumberStyles.HexNumber, null, out var bankId))
        {
            Log.Error("Invalid bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)");

            return;
        }

        using var browser = new PackageBrowser();

        var soundsPackagePath = GetSoundsPackagePath(gameDir);

        if (!browser.LoadPackage(soundsPackagePath))
        {
            Log.Error("Failed to load sounds package");

            return;
        }

        // Try to load sound table for cue names
        var soundTablePath = FindSoundTableXml(gameDir);

        if (soundTablePath is not null)
        {
            browser.LoadSoundTable(soundTablePath);
        }

        var sounds = browser.GetSounds(bankId).ToList();

        if (sounds.Count == 0)
        {
            Log.Warn("No sounds found in bank {0:X8}", bankId);

            return;
        }

        Log.Info("");
        Log.Info("Sounds in bank {0:X8}:", bankId);
        Log.Info("------------------------");

        foreach (var sound in sounds.OrderBy(s => s.SourceId))
        {
            var embedded = sound.HasEmbeddedMedia ? "[embedded]" : "[streaming]";
            var name = sound.Name is not null ? $" ({sound.Name})" : "";
            Log.Info("  {0}{1} - {2} {3}", sound.SourceIdHex, name, sound.StreamType, embedded);
        }

        Log.Info("");
        Log.Info("Total: {0} sounds", sounds.Count);
    }

    private static void RunProjectCreate(string name, string filePath, string? gameDirArg)
    {
        var project = ProjectFile.Create(name);

        // Try to find game directory
        var gameDir = gameDirArg ?? FindHaloWarsGameDirectory();

        if (gameDir is not null)
        {
            project.GameDirectory = gameDir;
            project.PackagePath = GetSoundsPackagePath(gameDir);

            var soundTablePath = FindSoundTableXml(gameDir);

            if (soundTablePath is not null)
            {
                project.SoundTablePath = soundTablePath;
            }
        }

        if (project.Save(filePath))
        {
            Log.Info("Project created: {0}", filePath);
            Log.Info("  Name: {0}", project.Name);

            if (project.GameDirectory is not null)
            {
                Log.Info("  Game Directory: {0}", project.GameDirectory);
            }

            if (project.PackagePath is not null)
            {
                Log.Info("  Package Path: {0}", project.PackagePath);
            }

            if (project.SoundTablePath is not null)
            {
                Log.Info("  Sound Table: {0}", project.SoundTablePath);
            }
        }
        else
        {
            Log.Error("Failed to create project file");
        }
    }

    private static void RunProjectInfo(string projectPath)
    {
        var project = ProjectFile.Load(projectPath);

        if (project is null)
        {
            Log.Error("Failed to load project: {0}", projectPath);

            return;
        }

        Log.Info("=== Project: {0} ===", project.Name);
        Log.Info("");
        Log.Info("File:           {0}", projectPath);
        Log.Info("Created:        {0}", project.CreatedAt.ToLocalTime());
        Log.Info("Modified:       {0}", project.ModifiedAt.ToLocalTime());
        Log.Info("");

        if (project.GameDirectory is not null)
        {
            Log.Info("Game Directory: {0}", project.GameDirectory);
        }

        if (project.PackagePath is not null)
        {
            Log.Info("Package Path:   {0}", project.PackagePath);
        }

        if (project.SoundTablePath is not null)
        {
            Log.Info("Sound Table:    {0}", project.SoundTablePath);
        }

        if (project.OutputDirectory is not null)
        {
            Log.Info("Output Dir:     {0}", project.OutputDirectory);
        }

        if (project.EditingBanks.Count > 0)
        {
            Log.Info("");
            Log.Info("Editing Banks ({0}):", project.EditingBanks.Count);

            foreach (var bankId in project.EditingBanks)
            {
                Log.Info("  {0:X8}", bankId);
            }
        }

        if (project.EditingSounds.Count > 0)
        {
            Log.Info("");
            Log.Info("Editing Sounds ({0}):", project.EditingSounds.Count);

            foreach (var soundId in project.EditingSounds)
            {
                Log.Info("  {0:X8}", soundId);
            }
        }

        if (!string.IsNullOrWhiteSpace(project.Notes))
        {
            Log.Info("");
            Log.Info("Notes:");
            Log.Info("  {0}", project.Notes);
        }
    }
}
