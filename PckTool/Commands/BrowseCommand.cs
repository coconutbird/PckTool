using System.ComponentModel;

using PckTool.Core.Games;
using PckTool.Core.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the browse command.
/// </summary>
public class BrowseSettings : GlobalSettings
{
    [Description("Specific bank ID (decimal or hex with 0x prefix) to show details for.")] [CommandOption("-b|--bank")]
    public string? Bank { get; init; }

    [Description("Filter by language (e.g., 'English(US)', 'SFX').")] [CommandOption("-l|--language")]
    public string? Language { get; init; }
}

/// <summary>
///     Browse sound banks in the package file.
/// </summary>
public class BrowseCommand : Command<BrowseSettings>
{
    public override int Execute(CommandContext context, BrowseSettings settings)
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

            if (soundTablePath is not null && browser.LoadSoundTable(soundTablePath))
            {
                AnsiConsole.MarkupLine("[blue]Sound table loaded[/]");
            }
        }

        // Parse bank ID filter if provided
        uint? bankId = null;

        if (!string.IsNullOrWhiteSpace(settings.Bank))
        {
            if (GameHelpers.TryParseId(settings.Bank, out var parsedId))
            {
                bankId = parsedId;
            }
            else
            {
                AnsiConsole.MarkupLine(
                    "[red]Invalid bank ID format. Use decimal (e.g., 12345) or hex with 0x prefix (e.g., 0x1A2B3C4D)[/]");

                return 1;
            }
        }

        // If a specific bank is requested, show details
        if (bankId.HasValue)
        {
            return ShowBankDetails(browser, bankId.Value);
        }

        // Otherwise, list all banks
        return ListAllBanks(browser, settings.Language);
    }

    private static int ShowBankDetails(PackageBrowser browser, uint bankId)
    {
        var details = browser.GetBankDetails(bankId);

        if (details is null)
        {
            AnsiConsole.MarkupLine($"[red]Bank {bankId:X8} not found[/]");

            return 1;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]=== Bank Details: {details.IdHex} ===[/]");

        var infoTable = new Table();
        infoTable.AddColumn("Property");
        infoTable.AddColumn("Value");
        infoTable.AddRow("Language", details.Language);
        infoTable.AddRow("Size", details.SizeFormatted);
        infoTable.AddRow("Version", $"0x{details.Version:X}");
        infoTable.AddRow("Valid", details.IsValid.ToString());
        infoTable.AddRow("Sounds", details.SoundCount.ToString());
        infoTable.AddRow("Events", details.EventCount.ToString());
        infoTable.AddRow("Actions", details.ActionCount.ToString());
        infoTable.AddRow("Media Files", details.MediaCount.ToString());
        AnsiConsole.Write(infoTable);

        if (details.Sounds.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Sounds:[/]");

            var soundsTable = new Table();
            soundsTable.AddColumn("Source ID");
            soundsTable.AddColumn("Name");
            soundsTable.AddColumn("Type");

            foreach (var sound in details.Sounds)
            {
                var embedded = sound.HasEmbeddedMedia ? "[blue]embedded[/]" : "[yellow]streaming[/]";
                soundsTable.AddRow(sound.SourceIdHex, sound.DisplayName, embedded);
            }

            AnsiConsole.Write(soundsTable);
        }

        return 0;
    }

    private static int ListAllBanks(PackageBrowser browser, string? languageFilter)
    {
        // Parse language filter
        uint? languageId = null;

        if (!string.IsNullOrWhiteSpace(languageFilter))
        {
            foreach (var (id, name) in browser.Languages)
            {
                if (name.Equals(languageFilter, StringComparison.OrdinalIgnoreCase))
                {
                    languageId = id;

                    break;
                }
            }

            if (!languageId.HasValue)
            {
                AnsiConsole.MarkupLine($"[yellow]Language '{languageFilter}' not found, showing all languages[/]");
            }
        }

        var banks = browser.GetBanks(languageId).ToList();
        var banksByLanguage = banks.GroupBy(b => b.Language).OrderBy(g => g.Key);

        AnsiConsole.WriteLine();
        var table = new Table();
        table.AddColumn("Bank ID");
        table.AddColumn("Language");
        table.AddColumn("Size");
        table.AddColumn("Sounds");

        foreach (var group in banksByLanguage)
        {
            foreach (var bank in group.OrderBy(b => b.Id))
            {
                table.AddRow($"[blue]{bank.IdHex}[/]", group.Key, bank.SizeFormatted, bank.SoundCount.ToString());
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Total:[/] {banks.Count} banks");

        return 0;
    }
}
