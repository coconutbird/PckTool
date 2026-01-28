using System.ComponentModel;
using System.Globalization;

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
        var gameDir = GameHelpers.ResolveGameDirectory(settings.GameDir);

        if (gameDir is null)
        {
            AnsiConsole.MarkupLine("[red]Failed to find Halo Wars game directory[/]");

            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Found Halo Wars game directory:[/] {gameDir}");

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

        var soundsPackagePath = GameHelpers.GetSoundsPackagePath(gameDir);

        try
        {
            var package = ServiceProvider.PckFileFactory.Load(soundsPackagePath);

            // Find the sound bank entry to replace
            var entry = package.SoundBanks[bankId];

            if (entry is null)
            {
                AnsiConsole.MarkupLine($"[red]Sound bank {bankId:X8} not found in package[/]");

                return 1;
            }

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
                outputFile = Path.Join(settings.Output, "Sounds_modified.pck");
            }

            GameHelpers.EnsureDirectoryCreated(outputFile);

            // Save the modified package
            AnsiConsole.MarkupLine($"[blue]Saving modified package to:[/] {outputFile}");
            package.Save(outputFile);

            AnsiConsole.MarkupLine($"[green]Done![/] Sound bank [blue]{bankId:X8}[/] has been replaced.");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load sounds file:[/] {ex.Message}");

            return 1;
        }
    }
}
