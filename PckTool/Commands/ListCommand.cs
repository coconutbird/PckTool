using System.Diagnostics.CodeAnalysis;

using PckTool.Abstractions;
using PckTool.Core.Games;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     List all sound banks in the package file.
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
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
            // Load all files into a composite set for unified view
            var shouldUseFileSet = resolution.Files.Count > 1;

            AnsiConsole.MarkupLine(
                shouldUseFileSet
                    ? $"[blue]Loading {resolution.Files.Count} files as composite set[/]"
                    : "[blue]Loading single file[/]");

            using var audioFile = shouldUseFileSet
                ? ServiceProvider.AudioFileFactory.Load(resolution.GameDir!, true)
                : ServiceProvider.AudioFileFactory.Load(resolution.Files[0]);

            AnsiConsole.MarkupLine(
                shouldUseFileSet
                    ? $"[green]Loaded {audioFile.SoundBankCount} sound banks from {resolution.Files.Count} files[/]"
                    : $"[green]Loaded {audioFile.SoundBankCount} sound banks from {resolution.Files[0]}[/]");

            // Create a table for output
            var table = new Table();
            table.AddColumn("Bank ID");
            table.AddColumn("Language");
            table.AddColumn("Size");

            // Add source file column for composite sets
            if (shouldUseFileSet)
            {
                table.AddColumn("Source File");
            }

            // Group by language for cleaner output
            var banksByLanguage = audioFile.SoundBanks
                                           .Entries
                                           .GroupBy(e => audioFile.GetLanguageNameOrDefault(e.LanguageId))
                                           .OrderBy(g => g.Key);

            foreach (var languageGroup in banksByLanguage)
            {
                foreach (var entry in languageGroup.OrderBy(e => e.Id))
                {
                    if (shouldUseFileSet)
                    {
                        var sourceFileName = entry.ParentFile?.SourcePath is not null && resolution.GameDir is not null
                            ? Path.GetRelativePath(resolution.GameDir, entry.ParentFile.SourcePath)
                            : entry.ParentFile?.SourcePath is not null
                                ? Path.GetFileName(entry.ParentFile.SourcePath)
                                : "[grey]Unknown[/]";

                        table.AddRow(
                            $"[blue]{entry.Id:X8}[/]",
                            languageGroup.Key,
                            $"{entry.Size:N0} bytes",
                            sourceFileName);
                    }
                    else
                    {
                        table.AddRow(
                            $"[blue]{entry.Id:X8}[/]",
                            languageGroup.Key,
                            $"{entry.Size:N0} bytes");
                    }
                }
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Total:[/] {audioFile.SoundBankCount} sound banks");
            AnsiConsole.MarkupLine($"[bold]Streaming files:[/] {audioFile.StreamingFileCount}");
            AnsiConsole.MarkupLine($"[bold]External files:[/] {audioFile.ExternalFileCount}");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load audio file:[/] {ex.Message}");

            return 1;
        }
    }
}
