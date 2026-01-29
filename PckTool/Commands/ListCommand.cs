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

        var inputFiles = resolution.Metadata.GetDefaultInputFiles(resolution.GameDir).ToList();

        if (inputFiles.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]No audio files found for {resolution.Game.ToDisplayName()}[/]");

            return 1;
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

            foreach (var inputFile in inputFiles)
            {
                var absolutePath = Path.Combine(resolution.GameDir, inputFile);
                AnsiConsole.MarkupLine($"[blue]Loading:[/] {inputFile}");

                var package = ServiceProvider.PckFileFactory.Load(absolutePath);

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
                            inputFile);
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
