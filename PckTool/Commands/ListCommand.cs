using PckTool.Core.Games;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     List all sound banks in the package file.
/// </summary>
public class ListCommand : Command<GlobalSettings>
{
    public override int Execute(CommandContext context, GlobalSettings settings)
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

        try
        {
            // Create a table for output
            var table = new Table();
            table.AddColumn("Bank ID");
            table.AddColumn("Language");
            table.AddColumn("Size");
            table.AddColumn("File");

            var totalBanks = 0;

            foreach (var filePath in resolution.Files)
            {
                AnsiConsole.MarkupLine($"[blue]Loading:[/] {Path.GetFileName(filePath)}");

                var package = ServiceProvider.PckFileFactory.Load(filePath);

                // Group by language for cleaner output
                var banksByLanguage = package.SoundBanks
                                             .Entries
                                             .GroupBy(e => package.Languages[e.LanguageId])
                                             .OrderBy(g => g.Key);

                foreach (var languageGroup in banksByLanguage)
                {
                    foreach (var entry in languageGroup.OrderBy(e => e.Id))
                    {
                        table.AddRow(
                            $"[blue]{entry.Id:X8}[/]",
                            languageGroup.Key,
                            $"{entry.Size:N0} bytes",
                            Path.GetFileName(filePath));
                    }
                }

                totalBanks += package.SoundBanks.Count;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Total:[/] {totalBanks} sound banks");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load audio file:[/] {ex.Message}");

            return 1;
        }
    }
}
