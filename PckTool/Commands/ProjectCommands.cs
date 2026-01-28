using System.ComponentModel;

using PckTool.Core.Services;

using Spectre.Console;
using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Settings for the project create command.
/// </summary>
public class ProjectCreateSettings : GlobalSettings
{
    [Description("Project name.")] [CommandOption("-n|--name")] [DefaultValue("Untitled Project")]
    public string Name { get; init; } = "Untitled Project";

    [Description("Path to save the project file.")] [CommandOption("-f|--file")]
    public required string File { get; init; }
}

/// <summary>
///     Create a new project file.
/// </summary>
public class ProjectCreateCommand : Command<ProjectCreateSettings>
{
    public override int Execute(CommandContext context, ProjectCreateSettings settings)
    {
        var project = ProjectFile.Create(settings.Name);

        // Try to find game directory
        var gameDir = GameHelpers.ResolveGameDirectory(settings.GameDir);

        if (gameDir is not null)
        {
            project.GameDirectory = gameDir;
            project.PackagePath = GameHelpers.GetSoundsPackagePath(gameDir);

            var soundTablePath = GameHelpers.FindSoundTableXml(gameDir);

            if (soundTablePath is not null)
            {
                project.SoundTablePath = soundTablePath;
            }
        }

        if (project.Save(settings.File))
        {
            AnsiConsole.MarkupLine($"[green]Project created:[/] {settings.File}");

            var table = new Table();
            table.AddColumn("Property");
            table.AddColumn("Value");
            table.AddRow("Name", project.Name);

            if (project.GameDirectory is not null)
            {
                table.AddRow("Game Directory", project.GameDirectory);
            }

            if (project.PackagePath is not null)
            {
                table.AddRow("Package Path", project.PackagePath);
            }

            if (project.SoundTablePath is not null)
            {
                table.AddRow("Sound Table", project.SoundTablePath);
            }

            AnsiConsole.Write(table);

            return 0;
        }

        AnsiConsole.MarkupLine("[red]Failed to create project file[/]");

        return 1;
    }
}

/// <summary>
///     Settings for the project info command.
/// </summary>
public class ProjectInfoSettings : CommandSettings
{
    [Description("Path to the project file.")] [CommandOption("-p|--project")]
    public required string Project { get; init; }
}

/// <summary>
///     Show project information.
/// </summary>
public class ProjectInfoCommand : Command<ProjectInfoSettings>
{
    public override int Execute(CommandContext context, ProjectInfoSettings settings)
    {
        var project = ProjectFile.Load(settings.Project);

        if (project is null)
        {
            AnsiConsole.MarkupLine($"[red]Failed to load project:[/] {settings.Project}");

            return 1;
        }

        AnsiConsole.MarkupLine($"[bold]=== Project: {project.Name} ===[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("Property");
        table.AddColumn("Value");
        table.AddRow("File", settings.Project);
        table.AddRow("Created", project.CreatedAt.ToLocalTime().ToString());
        table.AddRow("Modified", project.ModifiedAt.ToLocalTime().ToString());

        if (project.GameDirectory is not null)
        {
            table.AddRow("Game Directory", project.GameDirectory);
        }

        if (project.PackagePath is not null)
        {
            table.AddRow("Package Path", project.PackagePath);
        }

        if (project.SoundTablePath is not null)
        {
            table.AddRow("Sound Table", project.SoundTablePath);
        }

        if (project.OutputDirectory is not null)
        {
            table.AddRow("Output Directory", project.OutputDirectory);
        }

        AnsiConsole.Write(table);

        if (project.EditingBanks.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Editing Banks ({project.EditingBanks.Count}):[/]");

            foreach (var bankId in project.EditingBanks)
            {
                AnsiConsole.MarkupLine($"  [blue]{bankId:X8}[/]");
            }
        }

        if (project.EditingSounds.Count > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Editing Sounds ({project.EditingSounds.Count}):[/]");

            foreach (var soundId in project.EditingSounds)
            {
                AnsiConsole.MarkupLine($"  [blue]{soundId:X8}[/]");
            }
        }

        if (!string.IsNullOrWhiteSpace(project.Notes))
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Notes:[/]");
            AnsiConsole.MarkupLine($"  {project.Notes}");
        }

        return 0;
    }
}
