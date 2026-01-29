using PckTool.Commands;

using Spectre.Console.Cli;

namespace PckTool;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("PckTool");

            config.AddCommand<InfoCommand>("info")
                  .WithDescription("Show configuration paths and info.");

            config.AddCommand<ListCommand>("list")
                  .WithDescription("List all sound banks in the package file.");

            config.AddCommand<DumpCommand>("dump")
                  .WithDescription("Extract all sound banks from the game.");

            config.AddCommand<ReplaceCommand>("replace")
                  .WithDescription("Replace a sound bank in the package file.");

            config.AddCommand<ReplaceWemCommand>("replace-wem")
                  .WithDescription("Replace a WEM file in the package.");

            config.AddCommand<BrowseCommand>("browse")
                  .WithDescription("Browse sound banks in the package file.");

            config.AddCommand<SoundsCommand>("sounds")
                  .WithDescription("List all sounds in a specific bank.");

            // Batch project commands
            config.AddBranch(
                "batch",
                batch =>
                {
                    batch.SetDescription("Manage batch projects for automated operations.");

                    batch.AddCommand<BatchProjectCreateCommand>("create")
                         .WithDescription("Create a new batch project file.");

                    batch.AddCommand<BatchProjectInfoCommand>("info")
                         .WithDescription("Show batch project information.");

                    batch.AddCommand<BatchProjectRunCommand>("run")
                         .WithDescription("Execute a batch project.");

                    batch.AddCommand<BatchProjectAddActionCommand>("add-action")
                         .WithDescription("Add an action to a batch project.");

                    batch.AddCommand<BatchProjectRemoveActionCommand>("remove-action")
                         .WithDescription("Remove an action from a batch project by index.");

                    batch.AddCommand<BatchProjectSchemaCommand>("schema")
                         .WithDescription("Generate JSON schema for batch project files.");
                });
        });

        return app.Run(args);
    }
}
