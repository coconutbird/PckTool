using System.ComponentModel;

using PckTool.Core.Games;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the replace-wem command.
/// </summary>
public class ReplaceWemSettings : GlobalSettings
{
    [Description("Target WEM ID (decimal or hex with 0x prefix) to replace.")] [CommandOption("-t|--target")]
    public required string Target { get; init; }

    [Description("Source WEM ID or file path. If a file path, uses that file. If an ID, copies from that WEM.")]
    [CommandOption("-s|--source")]
    public string? Source { get; init; }

    [Description("Path to the replacement .wem file. Alternative to --source.")] [CommandOption("-i|--input")]
    public string? Input { get; init; }
}

/// <summary>
///     Replace a WEM file in the package.
/// </summary>
public class ReplaceWemCommand : Command<ReplaceWemSettings>
{
    public override int Execute(CommandContext context, ReplaceWemSettings settings)
    {
        var resolution = GameHelpers.ResolveInputFiles(settings);

        if (!resolution.Success)
        {
            AnsiConsole.MarkupLine($"[red]{resolution.Error}[/]");

            return 1;
        }

        if (resolution.Game.HasValue)
        {
            AnsiConsole.MarkupLine($"[green]Game:[/] {resolution.Game.Value.ToDisplayName()}");
            AnsiConsole.MarkupLine($"[green]Directory:[/] {resolution.GameDir}");
        }

        // Parse target WEM ID using standardized helper
        if (!GameHelpers.TryParseId(settings.Target, out var targetWemId))
        {
            AnsiConsole.MarkupLine(
                "[red]Invalid target WEM ID format. Use decimal (e.g., 12345) or hex with 0x prefix (e.g., 0x1A2B3C4D)[/]");

            return 1;
        }

        AnsiConsole.MarkupLine($"[blue]Target WEM ID:[/] {targetWemId} (0x{targetWemId:X8})");

        // Get replacement data
        byte[] replacementData;

        if (settings.Input is not null && File.Exists(settings.Input))
        {
            replacementData = File.ReadAllBytes(settings.Input);
            AnsiConsole.MarkupLine(
                $"[blue]Using replacement file:[/] {settings.Input} ({replacementData.Length} bytes)");
        }
        else if (settings.Source is not null)
        {
            if (File.Exists(settings.Source))
            {
                replacementData = File.ReadAllBytes(settings.Source);
                AnsiConsole.MarkupLine(
                    $"[blue]Using replacement file:[/] {settings.Source} ({replacementData.Length} bytes)");
            }
            else
            {
                // Parse as WEM ID - search across all input files
                if (!GameHelpers.TryParseId(settings.Source, out var sourceWemId))
                {
                    AnsiConsole.MarkupLine(
                        "[red]Invalid source WEM ID format. Use decimal (e.g., 12345) or hex with 0x prefix (e.g., 0x1A2B3C4D)[/]");

                    return 1;
                }

                AnsiConsole.MarkupLine($"[blue]Source WEM ID:[/] {sourceWemId} (0x{sourceWemId:X8})");

                byte[]? foundData = null;

                foreach (var filePath in resolution.Files)
                {
                    var tempPck = ServiceProvider.PckFileFactory.Load(filePath);

                    // Try streaming files first
                    var streamingEntry = tempPck.StreamingFiles[sourceWemId];

                    if (streamingEntry is not null)
                    {
                        foundData = streamingEntry.GetData();
                        AnsiConsole.MarkupLine(
                            $"[green]Found source WEM in streaming files[/] ({foundData.Length} bytes)");

                        break;
                    }

                    // Search embedded media
                    foreach (var bankEntry in tempPck.SoundBanks)
                    {
                        var bank = bankEntry.Parse();

                        if (bank is not null && bank.ContainsMedia(sourceWemId))
                        {
                            foundData = bank.GetMedia(sourceWemId);
                            AnsiConsole.MarkupLine(
                                $"[green]Found source WEM in soundbank[/] [blue]0x{bankEntry.Id:X8}[/] ({foundData?.Length ?? 0} bytes)");

                            break;
                        }
                    }

                    if (foundData is not null)
                    {
                        break;
                    }
                }

                if (foundData is null)
                {
                    AnsiConsole.MarkupLine(
                        $"[red]Source WEM {sourceWemId} (0x{sourceWemId:X8}) not found in any input file[/]");

                    return 1;
                }

                replacementData = foundData;
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Must specify either --source or --input for replacement data[/]");

            return 1;
        }

        try
        {
            // Process each input file to find and replace the WEM
            foreach (var filePath in resolution.Files)
            {
                if (!File.Exists(filePath))
                {
                    continue;
                }

                AnsiConsole.MarkupLine($"[blue]Loading:[/] {Path.GetFileName(filePath)}");
                var package = ServiceProvider.PckFileFactory.Load(filePath);

                // Replace the WEM using the unified API
                AnsiConsole.MarkupLine("[blue]Replacing WEM...[/]");
                var result = package.ReplaceWem(targetWemId, replacementData);

                if (!result.ReplacedInStreaming && result.EmbeddedBanksModified == 0)
                {
                    continue; // WEM not found in this file
                }

                // Create backup if requested
                if (settings.Backup)
                {
                    var backupPath = GameHelpers.CreateBackup(filePath);

                    if (backupPath is not null)
                    {
                        AnsiConsole.MarkupLine($"[blue]Backup created:[/] {backupPath}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Warning:[/] Failed to create backup");
                    }
                }

                AnsiConsole.WriteLine();
                var resultTable = new Table();
                resultTable.AddColumn("Metric");
                resultTable.AddColumn("Value");
                resultTable.AddRow("Replaced in streaming", result.ReplacedInStreaming.ToString());
                resultTable.AddRow("Embedded banks modified", result.EmbeddedBanksModified.ToString());
                resultTable.AddRow("HIRC references updated", result.HircReferencesUpdated.ToString());
                AnsiConsole.Write(resultTable);

                // Determine output path
                var outputFile = settings.Output;

                if (Directory.Exists(settings.Output))
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(filePath);
                    var extension = Path.GetExtension(filePath);
                    outputFile = Path.Combine(settings.Output, $"{originalFileName}_modified{extension}");
                }
                else if (!settings.Output.EndsWith(".pck", StringComparison.OrdinalIgnoreCase)
                         && !settings.Output.EndsWith(".bnk", StringComparison.OrdinalIgnoreCase))
                {
                    outputFile = settings.Output + Path.GetExtension(filePath);
                }

                GameHelpers.EnsureDirectoryCreated(outputFile);

                // Save the modified package
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[blue]Saving modified package to:[/] {outputFile}");
                package.Save(outputFile);

                AnsiConsole.MarkupLine(
                    $"[green]Done![/] WEM [blue]{targetWemId} (0x{targetWemId:X8})[/] has been replaced.");

                return 0;
            }

            AnsiConsole.MarkupLine($"[red]WEM {targetWemId} (0x{targetWemId:X8}) not found in any input file[/]");

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to replace WEM:[/] {ex.Message}");

            return 1;
        }
    }
}
