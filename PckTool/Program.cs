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

#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif

            // Info command
            config.AddCommand<InfoCommand>("info")
                  .WithDescription("Show configuration paths, game directories, and sound table locations.")
                  .WithExample("info", "--game", "hwde");

            // List command
            config.AddCommand<ListCommand>("list")
                  .WithAlias("ls")
                  .WithDescription("List all sound banks in the package file with their IDs and sizes.")
                  .WithExample("list", "--game", "hwde")
                  .WithExample("list", "-f", "Sounds.pck");

            // Dump command
            config.AddCommand<DumpCommand>("dump")
                  .WithAlias("extract")
                  .WithDescription("Extract sound banks and WEM audio files from the game package.")
                  .WithExample("dump", "--game", "hwde", "-o", "dumps/")
                  .WithExample("dump", "--game", "hwde", "-l", "English(US)", "-o", "dumps/english/")
                  .WithExample("dump", "--game", "hwde", "-b", "0x1A2B3C4D", "-o", "dumps/")
                  .WithExample("dump", "-f", "Sounds.pck", "-o", "output/");

            // Replace command (BNK replacement)
            config.AddCommand<ReplaceCommand>("replace")
                  .WithAlias("replace-bnk")
                  .WithDescription("Replace one or more sound banks (BNK) in the package file.")
                  .WithExample("replace", "--game", "hwde", "-t", "0x1A2B3C4D", "-s", "custom.bnk", "-o", "output/")
                  .WithExample("replace", "-f", "Sounds.pck", "-t", "0x1A2B3C4D", "-s", "custom.bnk", "-o", "output/");

            // Replace WEM command
            config.AddCommand<ReplaceWemCommand>("replace-wem")
                  .WithDescription("Replace one or more WEM audio files in the package.")
                  .WithExample("replace-wem", "--game", "hwde", "-t", "0x39E3B0F1", "-s", "custom.wem", "-o", "output/")
                  .WithExample("replace-wem", "-f", "Sounds.pck", "-t", "970927665", "-s", "voice.wem", "-o", "./")
                  .WithExample(
                      "replace-wem",
                      "--game",
                      "hwde",
                      "-t",
                      "0x111",
                      "-s",
                      "a.wem",
                      "-t",
                      "0x222",
                      "-s",
                      "b.wem",
                      "-o",
                      "output/");

            // Browse command
            config.AddCommand<BrowseCommand>("browse")
                  .WithDescription("Browse sound banks and view their contents interactively.")
                  .WithExample("browse", "--game", "hwde")
                  .WithExample("browse", "--game", "hwde", "-b", "0x1A2B3C4D")
                  .WithExample("browse", "-f", "custom.bnk");

            // Sounds command
            config.AddCommand<SoundsCommand>("sounds")
                  .WithDescription("List all sounds (WEMs) in a specific sound bank with their IDs and cue names.")
                  .WithExample("sounds", "--game", "hwde", "-b", "0x1A2B3C4D")
                  .WithExample("sounds", "-f", "Sounds.pck", "-b", "0x1A2B3C4D");

            // Find command
            config.AddCommand<FindCommand>("find")
                  .WithAlias("search")
                  .WithDescription("Search for a WEM ID or cue name across all sound banks.")
                  .WithExample("find", "--game", "hwde", "-w", "0x39E3B0F1")
                  .WithExample("find", "--game", "hwde", "-w", "970927665")
                  .WithExample("find", "--game", "hwde", "-n", "play_explosion")
                  .WithExample("find", "-f", "Sounds.pck", "-w", "0x39E3B0F1");

            // Batch project commands
            config.AddBranch(
                "batch",
                batch =>
                {
                    batch.SetDescription("Manage batch projects for automated multi-file operations.");

                    batch.AddCommand<BatchProjectCreateCommand>("create")
                         .WithDescription("Create a new batch project file for organizing replacements.")
                         .WithExample("batch", "create", "mymod.json", "-n", "My Mod", "--game", "hwde")
                         .WithExample(
                             "batch",
                             "create",
                             "mymod.json",
                             "-n",
                             "My Mod",
                             "-i",
                             "Sounds.pck",
                             "-o",
                             "output/");

                    batch.AddCommand<BatchProjectInfoCommand>("info")
                         .WithDescription("Show batch project details including actions and settings.")
                         .WithExample("batch", "info", "mymod.json")
                         .WithExample("batch", "info", "mymod.json", "--validate");

                    batch.AddCommand<BatchProjectRunCommand>("run")
                         .WithDescription("Execute all actions in a batch project.")
                         .WithExample("batch", "run", "mymod.json", "--dry-run")
                         .WithExample("batch", "run", "mymod.json")
                         .WithExample("batch", "run", "mymod.json", "-v");

                    batch.AddCommand<BatchProjectAddActionCommand>("add-action")
                         .WithDescription("Add a replacement action to a batch project.")
                         .WithExample("batch", "add-action", "mymod.json", "-t", "0x39E3B0F1", "-s", "custom.wem")
                         .WithExample(
                             "batch",
                             "add-action",
                             "mymod.json",
                             "--type",
                             "bnk",
                             "-t",
                             "0x1A2B3C4D",
                             "-s",
                             "custom.bnk");

                    batch.AddCommand<BatchProjectRemoveActionCommand>("remove-action")
                         .WithDescription("Remove an action from a batch project by its 1-based index.")
                         .WithExample("batch", "remove-action", "mymod.json", "1");

                    batch.AddCommand<BatchProjectSchemaCommand>("schema")
                         .WithDescription("Generate a JSON schema file for IDE autocompletion and validation.")
                         .WithExample("batch", "schema", "batch-project-schema.json");

                    batch.AddCommand<BatchProjectValidateCommand>("validate")
                         .WithDescription("Validate a batch project file and check that source files exist.")
                         .WithExample("batch", "validate", "mymod.json")
                         .WithExample("batch", "validate", "mymod.json", "--check-files");
                });
        });

        return app.Run(args);
    }
}
