using System.ComponentModel;
using System.Globalization;

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
    [Description("Sound bank ID (hex) to replace.")] [CommandOption("-s|--soundbank")]
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

        // Parse sound bank ID
        if (!uint.TryParse(settings.SoundBank, NumberStyles.HexNumber, null, out var bankId))
        {
            AnsiConsole.MarkupLine("[red]Invalid sound bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)[/]");

            return 1;
        }

        // Verify input file exists
        if (!File.Exists(settings.Input))
        {
            AnsiConsole.MarkupLine($"[red]Input file not found:[/] {settings.Input}");

            return 1;
        }

        var inputFiles = resolution.Metadata.GetDefaultInputFiles(resolution.GameDir).ToList();

        if (inputFiles.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No audio files found for {resolution.Game.ToDisplayName()}[/]");

            return 1;
        }

        try
        {
            // Process each input file to find the sound bank
            foreach (var inputFile in inputFiles)
            {
                var absolutePath = Path.Combine(resolution.GameDir, inputFile);
                var package = ServiceProvider.PckFileFactory.Load(absolutePath);

                // Find the sound bank entry to replace
                var entry = package.SoundBanks[bankId];

                if (entry is null)
                {
                    continue; // Try next file
                }

                AnsiConsole.MarkupLine($"[blue]Found in:[/] {inputFile}");
                AnsiConsole.MarkupLine(
                    $"[green]Found sound bank[/] [blue]{bankId:X8}[/] (Language: {package.Languages[entry.LanguageId]}, Size: {entry.Size} bytes)");

                // Read the replacement data
                var replacementData = File.ReadAllBytes(settings.Input);
                AnsiConsole.MarkupLine($"[blue]Replacement file size:[/] {replacementData.Length} bytes");

                // Replace the data
                entry.ReplaceWith(replacementData);

                // Determine output path
                var outputFile = settings.Output;

                if (Directory.Exists(settings.Output))
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(inputFile);
                    var extension = Path.GetExtension(inputFile);
                    outputFile = Path.Join(settings.Output, $"{originalFileName}_modified{extension}");
                }

                GameHelpers.EnsureDirectoryCreated(outputFile);

                // Save the modified package
                AnsiConsole.MarkupLine($"[blue]Saving modified package to:[/] {outputFile}");
                package.Save(outputFile);

                AnsiConsole.MarkupLine($"[green]Done![/] Sound bank [blue]{bankId:X8}[/] has been replaced.");

                return 0;
            }

            AnsiConsole.MarkupLine($"[red]Sound bank {bankId:X8} not found in any input file[/]");

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to process audio file:[/] {ex.Message}");

            return 1;
        }
    }
}
