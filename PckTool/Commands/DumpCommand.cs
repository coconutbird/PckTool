using System.ComponentModel;
using System.Globalization;

using PckTool.Core.Games;
using PckTool.Core.Games.HaloWars;
using PckTool.Core.WWise.Bnk;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the dump command.
/// </summary>
public class DumpSettings : GlobalSettings
{
    [Description("Specific sound bank ID (hex) to extract. If not specified, extracts all sound banks.")]
    [CommandOption("-s|--soundbank")]
    public string? SoundBank { get; init; }

    [Description("Filter by language (e.g., 'English(US)', 'SFX'). If not specified, extracts all languages.")]
    [CommandOption("-l|--language")]
    public string? Language { get; init; }
}

/// <summary>
///     Extract all sound banks and WEM files from the game.
/// </summary>
public class DumpCommand : Command<DumpSettings>
{
    public override int Execute(CommandContext context, DumpSettings settings)
    {
        var resolution = GameHelpers.ResolveGame(settings.Game, settings.GameDir);

        if (resolution.Game == SupportedGame.Unknown || resolution.Metadata is null)
        {
            AnsiConsole.MarkupLine("[red]Game not specified or not supported[/]");
            AnsiConsole.MarkupLine("[dim]Use --game hwde to specify[/]");

            return 1;
        }

        if (resolution.GameDir is null)
        {
            AnsiConsole.MarkupLine("[red]Failed to find game directory[/]");
            AnsiConsole.MarkupLine("[dim]Use --game-dir to specify the game installation path[/]");

            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Game:[/] {resolution.Game.ToDisplayName()}");
        AnsiConsole.MarkupLine($"[green]Directory:[/] {resolution.GameDir}");

        var soundTablePath = GameHelpers.FindSoundTableXml(resolution.GameDir);

        if (soundTablePath is null)
        {
            AnsiConsole.MarkupLine("[yellow]Warning:[/] Failed to find sound table, cue names will not be resolved!");
        }

        var inputFiles = resolution.Metadata.GetDefaultInputFiles(resolution.GameDir).ToList();

        if (inputFiles.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No audio files found for {resolution.Game.ToDisplayName()}[/]");

            return 1;
        }

        try
        {
            // Process each input file
            foreach (var inputFile in inputFiles)
            {
                var absolutePath = Path.Combine(resolution.GameDir, inputFile);
                AnsiConsole.MarkupLine($"[blue]Processing:[/] {inputFile}");

                var package = ServiceProvider.PckFileFactory.Load(absolutePath);

                // Phase 1: Load all soundbanks
                var soundbanksByLanguage = new Dictionary<uint, Dictionary<uint, SoundBank>>();

                // Parse sound bank filter if provided
                uint? soundBankIdFilter = null;

                if (!string.IsNullOrWhiteSpace(settings.SoundBank))
                {
                    if (uint.TryParse(settings.SoundBank, NumberStyles.HexNumber, null, out var parsedId))
                    {
                        soundBankIdFilter = parsedId;
                        AnsiConsole.MarkupLine($"[blue]Filtering to sound bank:[/] {parsedId:X8}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(
                            "[red]Invalid sound bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)[/]");

                        return 1;
                    }
                }

                AnsiConsole.Status()
                           .Start(
                               "Loading soundbanks...",
                               ctx =>
                               {
                                   foreach (var fileEntry in package.SoundBanks)
                                   {
                                       var languageId = fileEntry.LanguageId;
                                       var language = package.Languages[languageId];

                                       // Apply language filter
                                       if (!string.IsNullOrWhiteSpace(settings.Language)
                                           && !language.Equals(settings.Language, StringComparison.OrdinalIgnoreCase))
                                       {
                                           continue;
                                       }

                                       // Apply sound bank filter
                                       if (soundBankIdFilter.HasValue && fileEntry.Id != soundBankIdFilter.Value)
                                       {
                                           continue;
                                       }

                                       ctx.Status($"Loading {fileEntry.Id:X8}...");

                                       var soundbank = SoundBank.Parse(fileEntry.GetData());

                                       if (soundbank is null || (soundbank.Media.Count == 0 && !soundbank.IsValid))
                                       {
                                           continue;
                                       }

                                       if (!soundbanksByLanguage.TryGetValue(languageId, out var languageBanks))
                                       {
                                           languageBanks = new Dictionary<uint, SoundBank>();
                                           soundbanksByLanguage[languageId] = languageBanks;
                                       }

                                       languageBanks[soundbank.Id] = soundbank;
                                   }
                               });

                // Phase 2: Load sound table and resolve file IDs
                var soundTable = new SoundTable();

                if (!string.IsNullOrWhiteSpace(soundTablePath) && soundTable.Load(soundTablePath))
                {
                    AnsiConsole.MarkupLine($"[green]Loaded {soundTable.Cues.Count} cue entries from sound table[/]");
                }

                // Build global lookup and resolve
                var globalBankLookup = new Dictionary<uint, SoundBank>();

                foreach (var languageBanks in soundbanksByLanguage.Values)
                {
                    foreach (var (bankId, soundbank) in languageBanks)
                    {
                        globalBankLookup.TryAdd(bankId, soundbank);
                    }
                }

                foreach (var (languageId, languageBanks) in soundbanksByLanguage)
                {
                    SoundBank? BankLookup(uint bankId)
                    {
                        if (languageBanks.TryGetValue(bankId, out var sameLanguageBank)) return sameLanguageBank;

                        return globalBankLookup.GetValueOrDefault(bankId);
                    }

                    foreach (var soundbank in languageBanks.Values)
                    {
                        soundTable.ResolveFileIds(soundbank, BankLookup);
                    }
                }

                AnsiConsole.MarkupLine(
                    $"[green]Resolved {soundTable.ResolvedFileIdCount} file IDs to cue references[/]");

                // Phase 3: Extract WEM files with progress
                AnsiConsole.Progress()
                           .Start(ctx =>
                           {
                               var task = ctx.AddTask("[green]Extracting WEM files...[/]");
                               var totalBanks = soundbanksByLanguage.Values.Sum(lb => lb.Count);
                               var processedBanks = 0;

                               foreach (var (languageId, languageBanks) in soundbanksByLanguage)
                               {
                                   var language = package.Languages[languageId];

                                   foreach (var (soundbankId, soundbank) in languageBanks)
                                   {
                                       var bankDir = Path.Join(settings.Output, language, $"{soundbankId:X8}");
                                       GameHelpers.EnsureDirectoryCreated(bankDir + Path.DirectorySeparatorChar);

                                       var metadata = new WemMetadata
                                       {
                                           SoundbankId = soundbankId, Language = language, LanguageId = languageId
                                       };

                                       foreach (var (wemId, wemData) in soundbank.Media)
                                       {
                                           var wemFile = Path.Join(bankDir, $"{wemId}.wem");
                                           File.WriteAllBytes(wemFile, wemData);

                                           var cueRefs = soundTable.GetCueReferencesByFileId(wemId);
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

                                           metadata.Files.Add(
                                               new WemFileEntry
                                               {
                                                   Id = wemId, Size = wemData.Length, Cues = cueMetadataList
                                               });
                                       }

                                       var metadataFile = Path.Join(bankDir, "metadata.json");
                                       metadata.Save(metadataFile);

                                       var bnkFile = Path.Join(settings.Output, language, $"{soundbankId:X8}.bnk");
                                       var fileEntry = package.SoundBanks.Entries.First(e => e.Id == soundbankId
                                           && e.LanguageId == languageId);

                                       File.WriteAllBytes(bnkFile, fileEntry.GetData());

                                       processedBanks++;
                                       task.Value = (double) processedBanks / totalBanks * 100;
                                   }
                               }

                               task.Value = 100;
                           });

                // Phase 4: Extract streaming files
                if (package.StreamingFiles.Count > 0)
                {
                    var streamingByLanguage = package.StreamingFiles
                                                     .Entries
                                                     .GroupBy(f => f.LanguageId)
                                                     .ToDictionary(g => g.Key, g => g.ToList());

                    AnsiConsole.Progress()
                               .Start(ctx =>
                               {
                                   var task = ctx.AddTask("[green]Extracting streaming WEM files...[/]");
                                   var totalFiles = package.StreamingFiles.Count;
                                   var processedFiles = 0;

                                   foreach (var (languageId, files) in streamingByLanguage)
                                   {
                                       var language = package.Languages.GetValueOrDefault(
                                           languageId,
                                           $"lang_{languageId}");

                                       var streamingDir = Path.Join(settings.Output, language, "_streaming");
                                       GameHelpers.EnsureDirectoryCreated(streamingDir + Path.DirectorySeparatorChar);

                                       var metadata = new StreamingMetadata
                                       {
                                           Language = language, LanguageId = languageId
                                       };

                                       foreach (var file in files)
                                       {
                                           var wemFile = Path.Join(streamingDir, $"{file.Id}.wem");
                                           var wemData = file.GetData();
                                           File.WriteAllBytes(wemFile, wemData);

                                           var cueRefs = soundTable.GetCueReferencesByFileId(file.Id);
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

                                           metadata.Files.Add(
                                               new StreamingFileMetadataEntry
                                               {
                                                   Id = file.Id, Size = wemData.Length, Cues = cueMetadataList
                                               });

                                           processedFiles++;
                                           task.Value = (double) processedFiles / totalFiles * 100;
                                       }

                                       var metadataFile = Path.Join(streamingDir, "metadata.json");
                                       metadata.Save(metadataFile);
                                   }

                                   task.Value = 100;
                               });
                }

                // Phase 5: Extract external files
                if (package.ExternalFiles.Count > 0)
                {
                    var externalByLanguage = package.ExternalFiles
                                                    .GroupBy(f => f.LanguageId)
                                                    .ToDictionary(g => g.Key, g => g.ToList());

                    AnsiConsole.Progress()
                               .Start(ctx =>
                               {
                                   var task = ctx.AddTask("[green]Extracting external WEM files...[/]");
                                   var totalFiles = package.ExternalFiles.Count;
                                   var processedFiles = 0;

                                   foreach (var (languageId, files) in externalByLanguage)
                                   {
                                       var language = package.Languages.GetValueOrDefault(
                                           languageId,
                                           $"lang_{languageId}");

                                       var externalDir = Path.Join(settings.Output, language, "_external");
                                       GameHelpers.EnsureDirectoryCreated(externalDir + Path.DirectorySeparatorChar);

                                       var metadata = new ExternalMetadata
                                       {
                                           Language = language, LanguageId = languageId
                                       };

                                       foreach (var file in files)
                                       {
                                           var wemFile = Path.Join(externalDir, $"{file.Id}.wem");
                                           var wemData = file.GetData();
                                           File.WriteAllBytes(wemFile, wemData);

                                           metadata.Files.Add(
                                               new ExternalFileMetadataEntry { Id = file.Id, Size = wemData.Length });

                                           processedFiles++;
                                           task.Value = (double) processedFiles / totalFiles * 100;
                                       }

                                       var metadataFile = Path.Join(externalDir, "metadata.json");
                                       metadata.Save(metadataFile);
                                   }

                                   task.Value = 100;
                               });
                }

                AnsiConsole.MarkupLine("[green]Done processing file![/]");
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load audio file:[/] {ex.Message}");

            return 1;
        }
    }
}
