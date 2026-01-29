using System.ComponentModel;
using System.Globalization;

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
        var gameDir = GameHelpers.ResolveGameDirectory(settings.GameDir);

        if (gameDir is null)
        {
            AnsiConsole.MarkupLine("[red]Failed to find Halo Wars game directory[/]");

            return 1;
        }

        // Parse bank ID
        if (!uint.TryParse(settings.Bank, NumberStyles.HexNumber, null, out var bankId))
        {
            AnsiConsole.MarkupLine("[red]Invalid bank ID format. Please use hexadecimal (e.g., 1A2B3C4D)[/]");

            return 1;
        }

        using var browser = new PackageBrowser();
        var soundsPackagePath = GameHelpers.GetSoundsPackagePath(gameDir);

        if (!browser.LoadPackage(soundsPackagePath))
        {
            AnsiConsole.MarkupLine("[red]Failed to load sounds package[/]");

            return 1;
        }

        // Try to load sound table for cue names
        var soundTablePath = GameHelpers.FindSoundTableXml(gameDir);

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
