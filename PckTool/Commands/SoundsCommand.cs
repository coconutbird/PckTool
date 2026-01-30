using System.ComponentModel;

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
    [Description("Bank ID (decimal or hex with 0x prefix) to list sounds from.")] [CommandOption("-b|--bank")]
    public required string Bank { get; init; }
}

/// <summary>
///     List all sounds in a specific bank.
/// </summary>
public class SoundsCommand : Command<SoundsSettings>
{
    public override int Execute(CommandContext context, SoundsSettings settings)
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

        // Parse bank ID using standardized helper
        if (!GameHelpers.TryParseId(settings.Bank, out var bankId))
        {
            AnsiConsole.MarkupLine(
                "[red]Invalid bank ID format. Use decimal (e.g., 12345) or hex with 0x prefix (e.g., 0x1A2B3C4D)[/]");

            return 1;
        }

        using var browser = new PackageBrowser();

        // Load each input file
        foreach (var filePath in resolution.Files)
        {
            if (!browser.LoadPackage(filePath))
            {
                AnsiConsole.MarkupLine($"[red]Failed to load audio file:[/] {Path.GetFileName(filePath)}");

                return 1;
            }
        }

        // Try to load sound table for cue names if we have a game directory
        if (resolution.GameDir is not null)
        {
            var soundTablePath = GameHelpers.FindSoundTableXml(resolution.GameDir);

            if (soundTablePath is not null)
            {
                browser.LoadSoundTable(soundTablePath);
            }
        }

        var sounds = browser.GetSounds(bankId).ToList();

        if (sounds.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No sounds found in bank 0x{bankId:X8}[/]");

            return 0;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Sounds in bank 0x{bankId:X8}:[/]");

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
