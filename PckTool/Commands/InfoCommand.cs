using PckTool.Core.Games;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Show default paths and configuration information.
/// </summary>
public class InfoCommand : Command<GlobalSettings>
{
    public override int Execute(CommandContext context, GlobalSettings settings)
    {
        AnsiConsole.MarkupLine("[bold]=== PckTool Configuration Info ===[/]");
        AnsiConsole.WriteLine();

        // Resolve game and directory
        var resolution = GameHelpers.ResolveGame(settings.Game, settings.GameDir);

        if (resolution.Game == SupportedGame.Unknown)
        {
            AnsiConsole.MarkupLine("[yellow]Game:[/] Not detected");
            AnsiConsole.MarkupLine("[dim]Use --game hwde or --game hw2 to specify a game[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]Game:[/] {resolution.Game.ToDisplayName()}");
        }

        AnsiConsole.WriteLine();

        // Game directory
        if (resolution.GameDir is not null)
        {
            AnsiConsole.MarkupLine($"[green]Game Directory:[/] {resolution.GameDir}");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Game Directory:[/] Not found (use --game-dir to specify)");
        }

        AnsiConsole.WriteLine();

        // Game-specific info
        if (resolution.GameDir is not null
            && resolution.Game != SupportedGame.Unknown
            && resolution.Metadata is not null)
        {
            var soundTablePath = GameHelpers.FindSoundTableXml(resolution.GameDir);

            if (soundTablePath is not null)
            {
                AnsiConsole.MarkupLine($"[green]Sound Table File:[/] {soundTablePath}");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Sound Table File:[/] Not found in game directory");
            }

            // Input files from game metadata
            var inputFiles = resolution.Metadata.GetDefaultInputFiles(resolution.GameDir).ToList();

            if (inputFiles.Count > 0)
            {
                AnsiConsole.MarkupLine("[green]Audio Files:[/]");

                foreach (var inputFile in inputFiles)
                {
                    var absolutePath = Path.Combine(resolution.GameDir, inputFile);

                    if (File.Exists(absolutePath))
                    {
                        var fileInfo = new FileInfo(absolutePath);
                        AnsiConsole.MarkupLine(
                            $"  [green]{inputFile}[/] ({fileInfo.Length:N0} bytes, {fileInfo.Length / 1024.0 / 1024.0:N2} MB)");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"  [yellow]{inputFile}[/] (not found)");
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Audio Files:[/] None found");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[blue]Default Output Directory:[/] dumps");

        return 0;
    }
}
