using System.ComponentModel;

using PckTool.Core.Games;
using PckTool.Core.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the find command.
/// </summary>
public class FindSettings : GlobalSettings
{
    [Description("WEM ID (decimal or hex with 0x prefix) to search for.")] [CommandOption("-w|--wem")]
    public string? WemId { get; init; }

    [Description("Cue name to search for (partial match supported).")] [CommandOption("-n|--name")]
    public string? Name { get; init; }

    [Description("Bank ID (decimal or hex with 0x prefix) to search within.")] [CommandOption("-b|--bank")]
    public string? Bank { get; init; }
}

/// <summary>
///     Find which bank contains a specific WEM ID or search by cue name.
/// </summary>
public class FindCommand : Command<FindSettings>
{
    public override int Execute(CommandContext context, FindSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.WemId) && string.IsNullOrWhiteSpace(settings.Name))
        {
            AnsiConsole.MarkupLine("[red]Must specify either --wem or --name to search for[/]");

            return 1;
        }

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

        using var browser = new PackageBrowser();

        foreach (var filePath in resolution.Files)
        {
            if (!browser.LoadPackage(filePath))
            {
                AnsiConsole.MarkupLine($"[red]Failed to load:[/] {Path.GetFileName(filePath)}");

                return 1;
            }
        }

        // Load sound table for cue names if available
        if (resolution.GameDir is not null)
        {
            var soundTablePath = GameHelpers.FindSoundTableXml(resolution.GameDir);

            if (soundTablePath is not null)
            {
                browser.LoadSoundTable(soundTablePath);
            }
        }

        // Parse bank filter if provided
        uint? bankFilter = null;

        if (!string.IsNullOrWhiteSpace(settings.Bank))
        {
            if (GameHelpers.TryParseId(settings.Bank, out var parsedBank))
            {
                bankFilter = parsedBank;
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Invalid bank ID. Use decimal or hex with 0x prefix[/]");

                return 1;
            }
        }

        AnsiConsole.WriteLine();

        // Search by WEM ID
        if (!string.IsNullOrWhiteSpace(settings.WemId))
        {
            if (!GameHelpers.TryParseId(settings.WemId, out var wemId))
            {
                AnsiConsole.MarkupLine("[red]Invalid WEM ID. Use decimal or hex with 0x prefix[/]");

                return 1;
            }

            return SearchByWemId(browser, wemId, bankFilter);
        }

        // Search by name
        return SearchByName(browser, settings.Name!, bankFilter);
    }

    private static int SearchByWemId(PackageBrowser browser, uint wemId, uint? bankFilter)
    {
        AnsiConsole.MarkupLine($"[bold]Searching for WEM ID:[/] {wemId} (0x{wemId:X8})");
        AnsiConsole.WriteLine();

        var results = new List<(BankInfo Bank, SoundInfo Sound)>();

        foreach (var bank in browser.GetBanks())
        {
            if (bankFilter.HasValue && bank.Id != bankFilter.Value)
            {
                continue;
            }

            foreach (var sound in browser.GetSounds(bank.Id))
            {
                if (sound.SourceId == wemId)
                {
                    results.Add((bank, sound));
                }
            }
        }

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]WEM {wemId} (0x{wemId:X8}) not found[/]");

            return 0;
        }

        var table = new Table();
        table.AddColumn("Bank ID");
        table.AddColumn("Language");
        table.AddColumn("Type");
        table.AddColumn("Cue Name");

        foreach (var (bank, sound) in results)
        {
            var streamType = sound.HasEmbeddedMedia ? "[blue]embedded[/]" : "[yellow]streaming[/]";
            var cueName = sound.Name ?? "(unknown)";

            table.AddRow($"[blue]0x{bank.Id:X8}[/]", bank.Language, streamType, Markup.Escape(cueName));
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Found in {results.Count} location(s)[/]");

        return 0;
    }

    private static int SearchByName(PackageBrowser browser, string name, uint? bankFilter)
    {
        AnsiConsole.MarkupLine($"[bold]Searching for cue name:[/] \"{name}\"");
        AnsiConsole.WriteLine();

        var results = new List<(BankInfo Bank, SoundInfo Sound)>();

        foreach (var bank in browser.GetBanks())
        {
            if (bankFilter.HasValue && bank.Id != bankFilter.Value)
            {
                continue;
            }

            foreach (var sound in browser.GetSounds(bank.Id))
            {
                if (sound.Name is not null && sound.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add((bank, sound));
                }
            }
        }

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No cues matching \"{name}\" found[/]");

            return 0;
        }

        var table = new Table();
        table.AddColumn("Cue Name");
        table.AddColumn("WEM ID");
        table.AddColumn("Bank ID");
        table.AddColumn("Language");

        foreach (var (bank, sound) in results.Take(50))
        {
            table.AddRow(
                Markup.Escape(sound.Name!),
                $"0x{sound.SourceId:X8}",
                $"[blue]0x{bank.Id:X8}[/]",
                bank.Language);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        if (results.Count > 50)
        {
            AnsiConsole.MarkupLine($"[dim]Showing first 50 of {results.Count} results[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold]Found {results.Count} matching cue(s)[/]");
        }

        return 0;
    }
}
