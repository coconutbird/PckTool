using System.ComponentModel;
using System.Globalization;

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
        var gameDir = GameHelpers.ResolveGameDirectory(settings.GameDir);

        if (gameDir is null)
        {
            AnsiConsole.MarkupLine("[red]Could not find Halo Wars game directory. Use --game-dir to specify.[/]");

            return 1;
        }

        // Parse target WEM ID
        uint targetWemId;

        if (settings.Target.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            targetWemId = uint.Parse(settings.Target[2..], NumberStyles.HexNumber);
        }
        else
        {
            targetWemId = uint.Parse(settings.Target);
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
                // Parse as WEM ID
                uint sourceWemId;

                if (settings.Source.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    sourceWemId = uint.Parse(settings.Source[2..], NumberStyles.HexNumber);
                }
                else
                {
                    sourceWemId = uint.Parse(settings.Source);
                }

                AnsiConsole.MarkupLine($"[blue]Source WEM ID:[/] {sourceWemId} (0x{sourceWemId:X8})");

                var soundsPackagePath = GameHelpers.GetSoundsPackagePath(gameDir);
                var tempPck = ServiceProvider.PckFileFactory.Load(soundsPackagePath);

                // Try streaming files first
                var streamingEntry = tempPck.StreamingFiles[sourceWemId];

                if (streamingEntry is not null)
                {
                    replacementData = streamingEntry.GetData();
                    AnsiConsole.MarkupLine(
                        $"[green]Found source WEM in streaming files[/] ({replacementData.Length} bytes)");
                }
                else
                {
                    // Search embedded media
                    byte[]? foundData = null;

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

                    if (foundData is null)
                    {
                        AnsiConsole.MarkupLine(
                            $"[red]Source WEM {sourceWemId} (0x{sourceWemId:X8}) not found in package[/]");

                        return 1;
                    }

                    replacementData = foundData;
                }
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Must specify either --source or --input for replacement data[/]");

            return 1;
        }

        // Load the package
        var packagePath = GameHelpers.GetSoundsPackagePath(gameDir);

        if (!File.Exists(packagePath))
        {
            AnsiConsole.MarkupLine($"[red]Sounds.pck not found at:[/] {packagePath}");

            return 1;
        }

        AnsiConsole.MarkupLine($"[blue]Loading package:[/] {packagePath}");

        try
        {
            var package = ServiceProvider.PckFileFactory.Load(packagePath);

            // Replace the WEM using the unified API
            AnsiConsole.MarkupLine("[blue]Replacing WEM...[/]");
            var result = package.ReplaceWem(targetWemId, replacementData);

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
                outputFile = Path.Combine(settings.Output, "Sounds_modified.pck");
            }
            else if (!settings.Output.EndsWith(".pck", StringComparison.OrdinalIgnoreCase))
            {
                outputFile = settings.Output + ".pck";
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
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to replace WEM:[/] {ex.Message}");

            return 1;
        }
    }
}
