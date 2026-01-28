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
        AnsiConsole.MarkupLine("[bold]=== SoundsUnpack Configuration Info ===[/]");
        AnsiConsole.WriteLine();

        // Game directory
        var gameDir = GameHelpers.FindHaloWarsGameDirectory();

        if (gameDir is not null)
        {
            AnsiConsole.MarkupLine($"[green]Default Game Directory:[/] {gameDir}");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Default Game Directory:[/] Not found (use --game-dir to specify)");
        }

        AnsiConsole.WriteLine();

        // Sound table
        if (gameDir is not null)
        {
            var soundTablePath = GameHelpers.FindSoundTableXml(gameDir);

            if (soundTablePath is not null)
            {
                AnsiConsole.MarkupLine($"[green]Sound Table File:[/] {soundTablePath}");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Sound Table File:[/] Not found in game directory");
            }

            // Sounds.pck path
            var soundsPackagePath = GameHelpers.GetSoundsPackagePath(gameDir);

            if (File.Exists(soundsPackagePath))
            {
                AnsiConsole.MarkupLine($"[green]Sounds Package:[/] {soundsPackagePath}");
                var fileInfo = new FileInfo(soundsPackagePath);
                AnsiConsole.MarkupLine(
                    $"  Size: {fileInfo.Length:N0} bytes ({fileInfo.Length / 1024.0 / 1024.0:N2} MB)");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Sounds Package:[/] Not found at expected path");
                AnsiConsole.MarkupLine($"  Expected: {soundsPackagePath}");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[blue]Default Output Directory:[/] dumps");

        return 0;
    }
}
