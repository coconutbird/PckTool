using System.ComponentModel;

using Spectre.Console.Cli;

namespace PckTool.Commands;

/// <summary>
///     Global settings shared across all commands.
/// </summary>
public class GlobalSettings : CommandSettings
{
  [Description("Game to use: 'hwde' (Halo Wars DE) or 'hw2' (Halo Wars 2).")] [CommandOption("--game")]
  public string? Game { get; init; }

  [Description("Path to the game directory. If not specified, attempts to find it automatically.")]
  [CommandOption("--game-dir")]
  public string? GameDir { get; init; }

  [Description("Direct path to a PCK or BNK file. Bypasses game detection.")] [CommandOption("-f|--file")]
  public string? File { get; init; }

  [Description("Output directory for extracted files.")] [CommandOption("-o|--output")] [DefaultValue("dumps")]
  public string Output { get; init; } = "dumps";

  [Description("Enable verbose output.")] [CommandOption("-v|--verbose")] [DefaultValue(false)]
  public bool Verbose { get; init; }

  [Description("Create a backup before modifying files.")] [CommandOption("--backup")] [DefaultValue(false)]
  public bool Backup { get; init; }
}
