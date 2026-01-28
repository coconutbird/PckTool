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
        var gameDir = GameHelpers.ResolveGameDirectory(settings.GameDir);

        if (gameDir is null)
        {
            AnsiConsole.MarkupLine("[red]Failed to find Halo Wars game directory[/]");

            return 1;
        }

        AnsiConsole.MarkupLine($"[green]Found Halo Wars game directory:[/] {gameDir}");

        var soundsPackagePath = GameHelpers.GetSoundsPackagePath(gameDir);

        try
        {
            var package = ServiceProvider.PckFileFactory.Load(soundsPackagePath);

            // Create a table for output
            var table = new Table();
            table.AddColumn("Bank ID");
            table.AddColumn("Language");
            table.AddColumn("Size");

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
                        $"{entry.Size:N0} bytes");
                }
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Total:[/] {package.SoundBanks.Count} sound banks");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load sounds file:[/] {ex.Message}");

            return 1;
        }
    }
}
