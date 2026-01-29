using System.ComponentModel;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using PckTool.Abstractions.Batch;
using PckTool.Core.Games;
using PckTool.Core.Services.Batch;
using PckTool.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

#region Settings

/// <summary>
///     Settings for batch project create command.
/// </summary>
public class BatchProjectCreateSettings : CommandSettings
{
    [Description("Project name.")] [CommandOption("-n|--name")] [DefaultValue("Untitled Batch Project")]
    public string Name { get; init; } = "Untitled Batch Project";

    [Description("Optional project description.")] [CommandOption("-d|--description")]
    public string? Description { get; init; }

    [Description("Path to save the project file.")] [CommandArgument(0, "<file>")]
    public required string File { get; init; }

    [Description("Input PCK or BNK files to process.")] [CommandOption("-i|--input")]
    public string[]? InputFiles { get; init; }

    [Description("Output directory for modified files.")] [CommandOption("-o|--output")]
    public string? OutputDirectory { get; init; }

    [Description("Game identifier (e.g., 'hwde' for Halo Wars DE). Used for auto-detecting input files.")]
    [CommandOption("-g|--game")]
    public string? Game { get; init; }

    [Description("Override game installation path (if auto-detection fails).")] [CommandOption("--game-path")]
    public string? GamePath { get; init; }
}

/// <summary>
///     Settings for batch project run command.
/// </summary>
public class BatchProjectRunSettings : CommandSettings
{
    [Description("Path to the batch project file.")] [CommandArgument(0, "<project>")]
    public required string Project { get; init; }

    [Description("Perform a dry run without making changes.")] [CommandOption("--dry-run")] [DefaultValue(false)]
    public bool DryRun { get; init; }

    [Description("Enable verbose output.")] [CommandOption("-v|--verbose")] [DefaultValue(false)]
    public bool Verbose { get; init; }
}

/// <summary>
///     Settings for batch project info command.
/// </summary>
public class BatchProjectInfoSettings : CommandSettings
{
    [Description("Path to the batch project file.")] [CommandArgument(0, "<project>")]
    public required string Project { get; init; }

    [Description("Validate the project configuration.")] [CommandOption("--validate")] [DefaultValue(false)]
    public new bool Validate { get; init; }
}

/// <summary>
///     Settings for adding an action to a batch project.
/// </summary>
public class BatchProjectAddActionSettings : CommandSettings
{
    [Description("Path to the batch project file.")] [CommandArgument(0, "<project>")]
    public required string Project { get; init; }

    [Description("Action type (replace, add, remove).")] [CommandOption("-a|--action")] [DefaultValue("replace")]
    public string ActionType { get; init; } = "replace";

    [Description("Target type (wem, bnk).")] [CommandOption("-t|--target-type")] [DefaultValue("wem")]
    public string TargetType { get; init; } = "wem";

    [Description("Target ID (decimal or hex with 0x prefix).")] [CommandOption("--id")]
    public required string TargetId { get; init; }

    [Description("Source file path for replace/add actions.")] [CommandOption("-s|--source")]
    public string? SourcePath { get; init; }

    [Description("Optional description for the action.")] [CommandOption("-d|--description")]
    public string? Description { get; init; }
}

/// <summary>
///     Settings for removing an action from a batch project.
/// </summary>
public class BatchProjectRemoveActionSettings : CommandSettings
{
    [Description("Path to the batch project file.")] [CommandArgument(0, "<project>")]
    public required string Project { get; init; }

    [Description("Index of the action to remove (1-based).")] [CommandArgument(1, "<index>")]
    public int Index { get; init; }
}

/// <summary>
///     Settings for batch project schema command.
/// </summary>
public class BatchProjectSchemaSettings : CommandSettings
{
    [Description("Output file path. If not specified, outputs to stdout.")] [CommandArgument(0, "[output]")]
    public string? OutputFile { get; init; }
}

#endregion

#region Commands

/// <summary>
///     Create a new batch project file.
/// </summary>
public class BatchProjectCreateCommand : Command<BatchProjectCreateSettings>
{
    public override int Execute(CommandContext context, BatchProjectCreateSettings settings)
    {
        var project = BatchProject.Create(settings.Name);
        project.Description = settings.Description;
        project.OutputDirectory = settings.OutputDirectory;
        project.Game = settings.Game;
        project.GamePath = settings.GamePath;

        if (settings.InputFiles is not null && settings.InputFiles.Length > 0)
        {
            // User explicitly specified input files
            foreach (var input in settings.InputFiles)
            {
                project.InputFiles.Add(input);
            }
        }
        else
        {
            // Auto-populate from game metadata (gamePath is only stored if explicitly provided)
            var resolution = GameHelpers.ResolveGame(settings.Game, settings.GamePath);

            if (resolution.GameDir is not null && project.GameMetadata is not null)
            {
                var defaultFiles = project.GameMetadata.GetDefaultInputFiles(resolution.GameDir);

                foreach (var file in defaultFiles)
                {
                    project.InputFiles.Add(file);
                }

                if (project.InputFiles.Count > 0)
                {
                    AnsiConsole.MarkupLine(
                        $"[dim]Auto-detected {project.InputFiles.Count} input file(s) for {project.ParsedGame.ToDisplayName()}[/]");
                }
            }
        }

        if (project.Save(settings.File))
        {
            AnsiConsole.MarkupLine($"[green]Batch project created:[/] {settings.File}");
            DisplayProjectInfo(project, settings.File);

            return 0;
        }

        AnsiConsole.MarkupLine("[red]Failed to create batch project file[/]");

        return 1;
    }

    private static void DisplayProjectInfo(BatchProject project, string path)
    {
        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");
        table.AddRow("Name", project.Name);
        table.AddRow("File", path);

        if (project.Description is not null) table.AddRow("Description", project.Description);

        table.AddRow("Schema Version", project.SchemaVersion.ToString());
        table.AddRow("Input Files", project.InputFiles.Count.ToString());
        table.AddRow("Actions", project.Actions.Count.ToString());

        if (project.OutputDirectory is not null) table.AddRow("Output Directory", project.OutputDirectory);

        AnsiConsole.Write(table);
    }
}

/// <summary>
///     Show batch project information.
/// </summary>
public class BatchProjectInfoCommand : Command<BatchProjectInfoSettings>
{
    public override int Execute(CommandContext context, BatchProjectInfoSettings settings)
    {
        var project = BatchProject.Load(settings.Project);

        if (project is null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load batch project:[/] {settings.Project}");

            return 1;
        }

        AnsiConsole.MarkupLine($"[bold]=== Batch Project: {project.Name} ===[/]");
        AnsiConsole.WriteLine();

        // Project info table
        var infoTable = new Table();
        infoTable.AddColumn("Property");
        infoTable.AddColumn("Value");
        infoTable.AddRow("File", settings.Project);
        infoTable.AddRow("Schema Version", project.SchemaVersion.ToString());

        if (project.Description is not null) infoTable.AddRow("Description", project.Description);

        if (project.OutputDirectory is not null) infoTable.AddRow("Output Directory", project.OutputDirectory);

        AnsiConsole.Write(infoTable);

        // Input files
        if (project.InputFiles.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Input Files ({project.InputFiles.Count}):[/]");

            foreach (var file in project.InputFiles)
            {
                AnsiConsole.MarkupLine($"  [blue]{file}[/]");
            }
        }

        // Actions
        if (project.Actions.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Actions ({project.Actions.Count}):[/]");

            var actionsTable = new Table();
            actionsTable.AddColumn("#");
            actionsTable.AddColumn("Type");
            actionsTable.AddColumn("Target");
            actionsTable.AddColumn("Source");
            actionsTable.AddColumn("Description");

            for (var i = 0; i < project.Actions.Count; i++)
            {
                var action = project.Actions[i];
                var (targetType, targetId, source) = GetActionDetails(action);
                actionsTable.AddRow(
                    (i + 1).ToString(),
                    action.ActionType.ToString(),
                    $"{targetType} 0x{targetId:X8}",
                    source ?? "-",
                    action.Description ?? "-");
            }

            AnsiConsole.Write(actionsTable);
        }

        // Validation
        if (settings.Validate)
        {
            AnsiConsole.WriteLine();
            var validation = project.Validate();

            if (validation.IsValid)
            {
                AnsiConsole.MarkupLine("[green]✓ Project validation passed[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]✗ Project validation failed:[/]");

                foreach (var error in validation.Errors)
                {
                    AnsiConsole.MarkupLine($"  [red]• {error}[/]");
                }

                return 1;
            }
        }

        return 0;
    }

    private static (string targetType, uint targetId, string? source) GetActionDetails(IProjectAction action)
    {
        return action switch
        {
            ReplaceAction r => (r.TargetType.ToString(), r.TargetId, r.SourcePath),
            AddAction a => (a.TargetType.ToString(), a.TargetId, a.SourcePath),
            RemoveAction r => (r.TargetType.ToString(), r.TargetId, null),
            _ => ("?", 0, null)
        };
    }
}

/// <summary>
///     Execute a batch project.
/// </summary>
public class BatchProjectRunCommand : Command<BatchProjectRunSettings>
{
    public override int Execute(CommandContext context, BatchProjectRunSettings settings)
    {
        var project = BatchProject.Load(settings.Project);

        if (project is null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load batch project:[/] {settings.Project}");

            return 1;
        }

        AnsiConsole.MarkupLine($"[bold]=== Running Batch Project: {project.Name} ===[/]");

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[yellow]DRY RUN - No changes will be made[/]");
        }

        AnsiConsole.WriteLine();

        // Validate first
        var validation = project.Validate();

        if (!validation.IsValid)
        {
            AnsiConsole.MarkupLine("[red]Project validation failed:[/]");

            foreach (var error in validation.Errors)
            {
                AnsiConsole.MarkupLine($"  [red]• {error}[/]");
            }

            return 1;
        }

        // Create executor
        var executor = new BatchProjectExecutor(ServiceProvider.PckFileFactory);

        // Subscribe to events for progress reporting
        executor.ActionStarted += (_, e) =>
        {
            var desc = e.Action.Description ?? $"{e.Action.ActionType}";
            AnsiConsole.MarkupLine($"[blue]({e.Index + 1}/{project.Actions.Count})[/] {desc}...");
        };

        executor.ActionCompleted += (_, e) =>
        {
            if (e.Result is not null)
            {
                if (e.Result.Success)
                {
                    AnsiConsole.MarkupLine($"  [green]✓[/] {e.Result.Message}");
                }
                else
                {
                    AnsiConsole.MarkupLine($"  [red]✗[/] {e.Result.Message}");
                }

                if (settings.Verbose && e.Result.WemResult is not null)
                {
                    var r = e.Result.WemResult;
                    AnsiConsole.MarkupLine(
                        $"    Streaming: {r.ReplacedInStreaming}, Banks: {r.EmbeddedBanksModified}, HIRC: {r.HircReferencesUpdated}");
                }
            }
        };

        // Execute
        try
        {
            var result = executor.Execute(project, settings.DryRun);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]{result.Summary}[/]");

            if (result.AllSucceeded)
            {
                AnsiConsole.MarkupLine("[green]Batch project completed successfully![/]");

                return 0;
            }

            AnsiConsole.MarkupLine("[yellow]Batch project completed with some failures.[/]");

            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error executing batch project:[/] {ex.Message}");

            return 1;
        }
    }
}

/// <summary>
///     Add an action to a batch project.
/// </summary>
public class BatchProjectAddActionCommand : Command<BatchProjectAddActionSettings>
{
    public override int Execute(CommandContext context, BatchProjectAddActionSettings settings)
    {
        var project = BatchProject.Load(settings.Project);

        if (project is null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load batch project:[/] {settings.Project}");

            return 1;
        }

        // Warn about unsupported action types
        var actionTypeLower = settings.ActionType.ToLowerInvariant();

        if (actionTypeLower is "add" or "remove")
        {
            AnsiConsole.MarkupLine(
                $"[yellow]Warning:[/] The '{settings.ActionType}' action type is not yet implemented. "
                + "Only 'replace' is currently supported.");

            if (!AnsiConsole.Confirm("Add this action anyway?", false))
            {
                return 0;
            }
        }

        // Parse target ID
        uint targetId;

        if (settings.TargetId.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            targetId = uint.Parse(settings.TargetId[2..], NumberStyles.HexNumber);
        }
        else
        {
            targetId = uint.Parse(settings.TargetId);
        }

        // Parse target type
        var targetType = settings.TargetType.ToLowerInvariant() switch
        {
            "wem" => TargetType.Wem,
            "bnk" => TargetType.Bnk,
            _ => throw new ArgumentException($"Unknown target type: {settings.TargetType}")
        };

        // Create action based on type
        IProjectAction action = actionTypeLower switch
        {
            "replace" => new ReplaceAction
            {
                TargetType = targetType,
                TargetId = targetId,
                SourcePath = settings.SourcePath,
                Description = settings.Description
            },
            "add" => new AddAction
            {
                TargetType = targetType,
                TargetId = targetId,
                SourcePath = settings.SourcePath,
                Description = settings.Description
            },
            "remove" => new RemoveAction
            {
                TargetType = targetType, TargetId = targetId, Description = settings.Description
            },
            _ => throw new ArgumentException($"Unknown action type: {settings.ActionType}")
        };

        project.Actions.Add(action);

        if (project.Save())
        {
            AnsiConsole.MarkupLine($"[green]Added {settings.ActionType} action for {targetType} 0x{targetId:X8}[/]");
            AnsiConsole.MarkupLine($"Project now has {project.Actions.Count} action(s).");

            return 0;
        }

        AnsiConsole.MarkupLine("[red]Failed to save project file[/]");

        return 1;
    }
}

/// <summary>
///     Remove an action from a batch project.
/// </summary>
public class BatchProjectRemoveActionCommand : Command<BatchProjectRemoveActionSettings>
{
    public override int Execute(CommandContext context, BatchProjectRemoveActionSettings settings)
    {
        var project = BatchProject.Load(settings.Project);

        if (project is null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load batch project:[/] {settings.Project}");

            return 1;
        }

        // Validate index (1-based)
        if (settings.Index < 1 || settings.Index > project.Actions.Count)
        {
            AnsiConsole.MarkupLine(
                $"[red]Invalid action index:[/] {settings.Index}. "
                + $"Project has {project.Actions.Count} action(s). Use 1-{project.Actions.Count}.");

            return 1;
        }

        var actionIndex = settings.Index - 1;
        var action = project.Actions[actionIndex];
        var (targetType, targetId, _) = GetActionDetails(action);

        project.Actions.RemoveAt(actionIndex);

        if (project.Save())
        {
            AnsiConsole.MarkupLine(
                $"[green]Removed action #{settings.Index}:[/] {action.ActionType} {targetType} 0x{targetId:X8}");

            AnsiConsole.MarkupLine($"Project now has {project.Actions.Count} action(s).");

            return 0;
        }

        AnsiConsole.MarkupLine("[red]Failed to save project file[/]");

        return 1;
    }

    private static (string targetType, uint targetId, string? source) GetActionDetails(IProjectAction action)
    {
        return action switch
        {
            ReplaceAction r => (r.TargetType.ToString(), r.TargetId, r.SourcePath),
            AddAction a => (a.TargetType.ToString(), a.TargetId, a.SourcePath),
            RemoveAction r => (r.TargetType.ToString(), r.TargetId, null),
            _ => ("?", 0, null)
        };
    }
}

/// <summary>
///     Generate JSON schema for batch project files.
/// </summary>
public class BatchProjectSchemaCommand : Command<BatchProjectSchemaSettings>
{
    public override int Execute(CommandContext context, BatchProjectSchemaSettings settings)
    {
        try
        {
            var schemaOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            var exporterOptions = new JsonSchemaExporterOptions
            {
                TreatNullObliviousAsNonNullable = true,
                TransformSchemaNode = (ctx, node) =>
                {
                    // Get description from [Description] attribute
                    var descAttr = ctx.PropertyInfo
                                      ?.AttributeProvider
                                      ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                      .OfType<DescriptionAttribute>()
                                      .FirstOrDefault();

                    if (descAttr is not null && node is JsonObject obj)
                    {
                        obj.Insert(0, "description", descAttr.Description);
                    }

                    return node;
                }
            };

            // Generate schema from BatchProject directly
            var schemaNode = schemaOptions.GetJsonSchemaAsNode(typeof(BatchProject), exporterOptions);

            // Generate schema for ReplaceAction (used for actions array items)
            var actionSchemaNode = schemaOptions.GetJsonSchemaAsNode(typeof(ReplaceAction), exporterOptions);

            // Replace the actions property's items schema with ReplaceAction schema
            if (schemaNode is JsonObject schemaObj && schemaObj["properties"] is JsonObject props)
            {
                if (props["actions"] is JsonObject actionsObj)
                {
                    actionsObj["items"] = actionSchemaNode.DeepClone();
                }

                // Add $schema property to the schema (for users to reference in their project files)
                var schemaPropertyObj = new JsonObject
                {
                    ["description"] = "JSON schema reference for IDE validation and autocomplete.",
                    ["type"] = "string"
                };

                props.Insert(0, "$schema", schemaPropertyObj);
            }

            // Add metadata at the top
            if (schemaNode is JsonObject obj)
            {
                var newObj = new JsonObject
                {
                    ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
                    ["$id"] = "https://github.com/coconutbird/PckTool/batch-project-schema.json",
                    ["title"] = "PckTool Batch Project",
                    ["description"] =
                        "Schema for PckTool batch project files that define operations on Wwise audio files."
                };

                foreach (var prop in obj)
                {
                    newObj[prop.Key] = prop.Value?.DeepClone();
                }

                schemaNode = newObj;
            }

            var outputOptions = new JsonSerializerOptions
            {
                WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var schemaJson = JsonSerializer.Serialize(schemaNode, outputOptions);

            if (!string.IsNullOrEmpty(settings.OutputFile))
            {
                File.WriteAllText(settings.OutputFile, schemaJson);
                AnsiConsole.MarkupLine($"[green]Schema written to:[/] {settings.OutputFile}");
            }
            else
            {
                Console.WriteLine(schemaJson);
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error generating schema:[/] {ex.Message}");

            return 1;
        }
    }
}

#endregion
