using System.ComponentModel;

using PckTool.Core.Games;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the replace-wem command.
/// </summary>
public class ReplaceWemSettings : GlobalSettings
{
    [Description("Target WEM ID(s) (decimal or hex with 0x prefix) to replace. Can specify multiple times.")]
    [CommandOption("-t|--target")]
    public string[] Targets { get; init; } = [];

    [Description("Path(s) to the replacement .wem file(s). Can specify multiple times (paired with --target).")]
    [CommandOption("-s|--source")]
    public string[] Sources { get; init; } = [];
}

/// <summary>
///     Replace one or more WEM files in the package.
/// </summary>
public class ReplaceWemCommand : Command<ReplaceWemSettings>
{
    public override int Execute(CommandContext context, ReplaceWemSettings settings)
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
        var replacements = new List<(uint TargetId, byte[] Data, string SourcePath)>();

        for (var i = 0; i < settings.Targets.Length; i++)
        {
            if (!GameHelpers.TryParseId(settings.Targets[i], out var targetId))
            {
                AnsiConsole.MarkupLine(
                    $"[red]Invalid target WEM ID format at position {i + 1}: {settings.Targets[i]}[/]");

                return 1;
            }

            var sourcePath = settings.Sources[i];

            if (!File.Exists(sourcePath))
            {
                AnsiConsole.MarkupLine($"[red]Source file not found:[/] {sourcePath}");

                return 1;
            }

            var data = File.ReadAllBytes(sourcePath);
            replacements.Add((targetId, data, sourcePath));
        }

        // Display replacement plan
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Replacement Plan ({replacements.Count} WEM(s)):[/]");
        var planTable = new Table();
        planTable.AddColumn("Target WEM ID");
        planTable.AddColumn("Source");
        planTable.AddColumn("Size");

        foreach (var (targetId, data, desc) in replacements)
        {
            planTable.AddRow($"0x{targetId:X8}", desc, $"{data.Length:N0} bytes");
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

                // Track if any replacements were made in this file
                var totalStreaming = 0;
                var totalBanks = 0;
                var totalHirc = 0;
                var replacedIds = new List<uint>();

                // Apply all replacements
                foreach (var (targetId, data, _) in replacements)
                {
                    var result = package.ReplaceWem(targetId, data);

                    if (result.WasReplaced)
                    {
                        replacedIds.Add(targetId);

                        if (result.ReplacedInStreaming)
                        {
                            totalStreaming++;
                        }

                        totalBanks += result.EmbeddedBanksModified;
                        totalHirc += result.HircReferencesUpdated;
                    }
                }

                if (replacedIds.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No matching WEMs found in this file[/]");

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
                resultTable.AddRow("WEMs replaced", replacedIds.Count.ToString());
                resultTable.AddRow("Streaming files modified", totalStreaming.ToString());
                resultTable.AddRow("Embedded banks modified", totalBanks.ToString());
                resultTable.AddRow("HIRC references updated", totalHirc.ToString());
                AnsiConsole.Write(resultTable);

                // Determine output path
                var outputFile = settings.Output;

                if (Directory.Exists(settings.Output))
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(filePath);
                    var extension = Path.GetExtension(filePath);
                    outputFile = Path.Combine(settings.Output, $"{originalFileName}_modified{extension}");
                }
                else if (!settings.Output.EndsWith(".pck", StringComparison.OrdinalIgnoreCase)
                         && !settings.Output.EndsWith(".bnk", StringComparison.OrdinalIgnoreCase))
                {
                    outputFile = settings.Output + Path.GetExtension(filePath);
                }

                GameHelpers.EnsureDirectoryCreated(outputFile);

                // Save the modified package
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[blue]Saving modified package to:[/] {outputFile}");
                package.Save(outputFile);

                var idsStr = string.Join(", ", replacedIds.Select(id => $"0x{id:X8}"));
                AnsiConsole.MarkupLine($"[green]Done![/] Replaced {replacedIds.Count} WEM(s): {idsStr}");

                return 0;
            }

            AnsiConsole.MarkupLine("[red]No WEMs were replaced in any input file[/]");

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to replace WEM:[/] {ex.Message}");

            return 1;
        }
    }
}
