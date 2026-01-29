using System.ComponentModel;
using System.Globalization;

using PckTool.Core.Games;
using PckTool.Core.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the sounds command.
/// </summary>
public class SoundsSettings : GlobalSettings
{
    [Description("Bank ID (hex) to list sounds from.")] [CommandOption("-b|--bank")]
    public required string Bank { get; init; }
}

/// <summary>
///     List all sounds in a specific bank.
/// </summary>
public class SoundsCommand : Command<SoundsSettings>
{
    public override int Execute(CommandContext context, SoundsSettings settings)
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

        // Parse bank ID
        if (!uint.TryParse(settings.Bank, NumberStyles.HexNumber, null, out var bankId))
        {
            AnsiConsole.MarkupLine("[red]Invalid bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)[/]");

            return 1;
        }

        var inputFiles = resolution.Metadata.GetDefaultInputFiles(resolution.GameDir).ToList();

        if (inputFiles.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No audio files found for {resolution.Game.ToDisplayName()}[/]");

            return 1;
        }

        using var browser = new PackageBrowser();

        // Load each input file
        foreach (var inputFile in inputFiles)
        {
            var absolutePath = Path.Combine(resolution.GameDir, inputFile);

            if (!browser.LoadPackage(absolutePath))
            {
                AnsiConsole.MarkupLine($"[red]Failed to load audio file:[/] {inputFile}");

                return 1;
            }
        }

        // Try to load sound table for cue names
        var soundTablePath = GameHelpers.FindSoundTableXml(resolution.GameDir);

        if (soundTablePath is not null)
        {
            browser.LoadSoundTable(soundTablePath);
        }

        var sounds = browser.GetSounds(bankId).ToList();

        if (sounds.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No sounds found in bank {bankId:X8}[/]");

            return 0;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Sounds in bank {bankId:X8}:[/]");

        var table = new Table();
        table.AddColumn("Source ID");
        table.AddColumn("Name");
        table.AddColumn("Type");
        table.AddColumn("Storage");

        foreach (var sound in sounds.OrderBy(s => s.SourceId))
        {
            var embedded = sound.HasEmbeddedMedia ? "[blue]embedded[/]" : "[yellow]streaming[/]";
            var name = sound.Name ?? "";
            table.AddRow(
                new Markup(sound.SourceIdHex),
                new Markup(Markup.Escape(name)),
                new Markup(sound.StreamType.ToString()),
                new Markup(embedded));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Total:[/] {sounds.Count} sounds");

        return 0;
    }
}
