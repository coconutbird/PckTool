using System.ComponentModel;

using PckTool.Core.Games;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the replace command.
/// </summary>
public class ReplaceSettings : GlobalSettings
{
    [Description("Sound bank ID(s) (decimal or hex with 0x prefix) to replace. Can specify multiple times.")]
    [CommandOption("-t|--target")]
    public string[] Targets { get; init; } = [];

    [Description("Path(s) to the replacement .bnk file(s). Can specify multiple times (paired with --target).")]
    [CommandOption("-s|--source")]
    public string[] Sources { get; init; } = [];
}

/// <summary>
///     Replace one or more sound banks in the package file.
/// </summary>
public class ReplaceCommand : Command<ReplaceSettings>
{
    public override int Execute(CommandContext context, ReplaceSettings settings)
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

        // Validate inputs
        if (settings.Targets.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Must specify at least one --target[/]");

            return 1;
        }

        if (settings.Sources.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]Must specify --source for each --target[/]");

            return 1;
        }

        if (settings.Targets.Length != settings.Sources.Length)
        {
            AnsiConsole.MarkupLine(
                $"[red]Mismatch: {settings.Targets.Length} target(s) but {settings.Sources.Length} source(s). Each --target needs a matching --source.[/]");

            return 1;
        }

        // Build list of replacements
        var replacements = new List<(uint BankId, string SourcePath, byte[] Data)>();

        for (var i = 0; i < settings.Targets.Length; i++)
        {
            if (!GameHelpers.TryParseId(settings.Targets[i], out var bankId))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Invalid sound bank ID format at position {i + 1}: {settings.Targets[i]}[/]");

                return 1;
            }

            var sourcePath = settings.Sources[i];

            if (!File.Exists(sourcePath))
            {
                AnsiConsole.MarkupLine($"[red]Source file not found:[/] {sourcePath}");

                return 1;
            }

            var data = File.ReadAllBytes(sourcePath);
            replacements.Add((bankId, sourcePath, data));
        }

        // Display replacement plan
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Replacement Plan ({replacements.Count} sound bank(s)):[/]");
        var planTable = new Table();
        planTable.AddColumn("Sound Bank ID");
        planTable.AddColumn("Source File");
        planTable.AddColumn("Size");

        foreach (var (bankId, sourcePath, data) in replacements)
        {
            planTable.AddRow($"0x{bankId:X8}", Path.GetFileName(sourcePath), $"{data.Length:N0} bytes");
        }

        AnsiConsole.Write(planTable);
        AnsiConsole.WriteLine();

        try
        {
            // Process each input file
            foreach (var filePath in resolution.Files)
            {
                if (!File.Exists(filePath))
                {
                    continue;
                }

                AnsiConsole.MarkupLine($"[blue]Loading:[/] {Path.GetFileName(filePath)}");
                var package = ServiceProvider.PckFileFactory.Load(filePath);

                // Track replacements made in this file
                var replacedBanks = new List<uint>();

                // Apply all replacements
                foreach (var (bankId, _, data) in replacements)
                {
                    var entry = package.SoundBanks[bankId];

                    if (entry is null)
                    {
                        continue;
                    }

                    entry.ReplaceWith(data);
                    replacedBanks.Add(bankId);
                    AnsiConsole.MarkupLine(
                        $"[green]Replaced sound bank[/] [blue]0x{bankId:X8}[/] (Language: {package.Languages[entry.LanguageId]})");
                }

                if (replacedBanks.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No matching sound banks found in this file[/]");

                    continue;
                }

                // Create backup if requested
                if (settings.Backup)
                {
                    var backupPath = GameHelpers.CreateBackup(filePath);

                    if (backupPath is not null)
                    {
                        AnsiConsole.MarkupLine($"[blue]Backup created:[/] {backupPath}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[yellow]Warning:[/] Failed to create backup");
                    }
                }

                // Display results
                AnsiConsole.WriteLine();
                var resultTable = new Table();
                resultTable.AddColumn("Metric");
                resultTable.AddColumn("Value");
                resultTable.AddRow("Sound banks replaced", replacedBanks.Count.ToString());
                AnsiConsole.Write(resultTable);

                // Determine output path
                var outputFile = settings.Output;

                if (Directory.Exists(settings.Output))
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(filePath);
                    var extension = Path.GetExtension(filePath);
                    outputFile = Path.Join(settings.Output, $"{originalFileName}_modified{extension}");
                }

                GameHelpers.EnsureDirectoryCreated(outputFile);

                // Save the modified package
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[blue]Saving modified package to:[/] {outputFile}");
                package.Save(outputFile);

                var idsStr = string.Join(", ", replacedBanks.Select(id => $"0x{id:X8}"));
                AnsiConsole.MarkupLine($"[green]Done![/] Replaced {replacedBanks.Count} sound bank(s): {idsStr}");

                return 0;
            }

            AnsiConsole.MarkupLine("[red]No sound banks were replaced in any input file[/]");

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to process audio file:[/] {ex.Message}");

            return 1;
        }
    }
}
