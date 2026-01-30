using System.ComponentModel;

using PckTool.Core.Games;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the replace command.
/// </summary>
public class ReplaceSettings : GlobalSettings
{
    [Description("Sound bank ID (decimal or hex with 0x prefix) to replace.")] [CommandOption("-s|--soundbank")]
    public required string SoundBank { get; init; }

    [Description("Path to the replacement .bnk file.")] [CommandOption("-i|--input")]
    public required string Input { get; init; }
}

/// <summary>
///     Replace a sound bank in the package file.
/// </summary>
public class ReplaceCommand : Command<ReplaceSettings>
{
    public override int Execute(CommandContext context, ReplaceSettings settings)
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

        // Parse sound bank ID using standardized helper
        if (!GameHelpers.TryParseId(settings.SoundBank, out var bankId))
        {
            AnsiConsole.MarkupLine(
                "[red]Invalid sound bank ID format. Use decimal (e.g., 12345) or hex with 0x prefix (e.g., 0x1A2B3C4D)[/]");

            return 1;
        }

        // Verify input file exists
        if (!File.Exists(settings.Input))
        {
            AnsiConsole.MarkupLine($"[red]Input file not found:[/] {settings.Input}");

            return 1;
        }

        try
        {
            // Process each input file to find the sound bank
            foreach (var filePath in resolution.Files)
            {
                var package = ServiceProvider.PckFileFactory.Load(filePath);

                // Find the sound bank entry to replace
                var entry = package.SoundBanks[bankId];

                if (entry is null)
                {
                    continue; // Try next file
                }

                AnsiConsole.MarkupLine($"[blue]Found in:[/] {Path.GetFileName(filePath)}");
                AnsiConsole.MarkupLine(
                    $"[green]Found sound bank[/] [blue]0x{bankId:X8}[/] (Language: {package.Languages[entry.LanguageId]}, Size: {entry.Size} bytes)");

                // Read the replacement data
                var replacementData = File.ReadAllBytes(settings.Input);
                AnsiConsole.MarkupLine($"[blue]Replacement file size:[/] {replacementData.Length} bytes");

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

                // Replace the data
                entry.ReplaceWith(replacementData);

                // Determine output path
                var outputFile = settings.Output;

                if (Directory.Exists(settings.Output))
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(filePath);
                    var extension = Path.GetExtension(filePath);
                    outputFile = Path.Join(settings.Output, $"{originalFileName}_modified{extension}");
                }

                GameHelpers.EnsureDirectoryCreated(outputFile);

                // Save the modified package
                AnsiConsole.MarkupLine($"[blue]Saving modified package to:[/] {outputFile}");
                package.Save(outputFile);

                AnsiConsole.MarkupLine($"[green]Done![/] Sound bank [blue]0x{bankId:X8}[/] has been replaced.");

                return 0;
            }

            AnsiConsole.MarkupLine($"[red]Sound bank 0x{bankId:X8} not found in any input file[/]");

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to process audio file:[/] {ex.Message}");

            return 1;
        }
    }
}
