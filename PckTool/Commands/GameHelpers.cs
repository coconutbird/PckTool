using System.Globalization;

using Microsoft.Win32;

using PckTool.Core.Games;

namespace PckTool.Commands;

/// <summary>
///     Helper methods for finding game files and directories.
/// </summary>
public static class GameHelpers
{
    /// <summary>
    ///     Parses an ID from a string, supporting both decimal and hex (with 0x prefix) formats.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">The parsed ID.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public static bool TryParseId(string? value, out uint result)
    {
        result = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Check for hex prefix
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return uint.TryParse(value[2..], NumberStyles.HexNumber, null, out result);
        }

        // Try decimal first, then hex without prefix
        if (uint.TryParse(value, out result))
        {
            return true;
        }

        // Try hex without prefix as fallback
        return uint.TryParse(value, NumberStyles.HexNumber, null, out result);
    }

    /// <summary>
    ///     Creates a backup of a file before modifying it.
    /// </summary>
    /// <param name="filePath">The file to back up.</param>
    /// <returns>The path to the backup file, or null if backup failed.</returns>
    public static string? CreateBackup(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(filePath) ?? ".";
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(directory, $"{fileName}_backup_{timestamp}{extension}");

        try
        {
            File.Copy(filePath, backupPath);

            return backupPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     Resolves input files from settings, supporting direct file, game detection, or both.
    /// </summary>
    /// <param name="settings">The global settings.</param>
    /// <returns>Resolution result with files and optional game info.</returns>
    public static FileResolutionResult ResolveInputFiles(GlobalSettings settings)
    {
        // If direct file is specified, use it
        if (!string.IsNullOrWhiteSpace(settings.File))
        {
            if (!File.Exists(settings.File))
            {
                return new FileResolutionResult([], null, null, $"File not found: {settings.File}");
            }

            return new FileResolutionResult([settings.File], null, null, null);
        }

        // Fall back to game-based resolution
        var gameResolution = ResolveGame(settings.Game, settings.GameDir);

        if (gameResolution.Game == SupportedGame.Unknown || gameResolution.Metadata is null)
        {
            return new FileResolutionResult(
                [],
                null,
                null,
                "Game not specified. Use --game hwde or --file <path>");
        }

        if (gameResolution.GameDir is null)
        {
            return new FileResolutionResult(
                [],
                gameResolution.Game,
                null,
                "Game directory not found. Use --game-dir <path> or --file <path>");
        }

        var inputFiles = gameResolution.Metadata.GetDefaultInputFiles(gameResolution.GameDir).ToList();

        if (inputFiles.Count == 0)
        {
            return new FileResolutionResult(
                [],
                gameResolution.Game,
                gameResolution.GameDir,
                $"No audio files found for {gameResolution.Game.ToDisplayName()}");
        }

        // Convert relative paths to absolute
        var absolutePaths = inputFiles
                            .Select(f => Path.Combine(gameResolution.GameDir, f))
                            .ToList();

        return new FileResolutionResult(absolutePaths, gameResolution.Game, gameResolution.GameDir, null);
    }

    /// <summary>
    ///     Resolves the game and directory from settings.
    /// </summary>
    /// <param name="gameArg">The --game argument value (required).</param>
    /// <param name="gameDirArg">The --game-dir argument value (optional, will try to find automatically).</param>
    /// <returns>Resolution result with game, directory, and metadata.</returns>
    public static GameResolutionResult ResolveGame(string? gameArg, string? gameDirArg)
    {
        if (string.IsNullOrWhiteSpace(gameArg))
        {
            return new GameResolutionResult(SupportedGame.Unknown, null, null);
        }

        var game = SupportedGameExtensions.ParseGame(gameArg);

        if (game == SupportedGame.Unknown)
        {
            return new GameResolutionResult(SupportedGame.Unknown, null, null);
        }

        var metadata = GameMetadata.GetMetadata(game);
        var gameDir = gameDirArg ?? FindGameDirectory(game);

        return new GameResolutionResult(game, gameDir, metadata);
    }

    /// <summary>
    ///     Finds the game directory for a specific game using registry or common paths.
    /// </summary>
    public static string? FindGameDirectory(SupportedGame game)
    {
        return game switch
        {
            SupportedGame.HaloWars => FindHaloWarsGameDirectory(),
            SupportedGame.HaloWars2 => FindHaloWars2GameDirectory(),
            _ => null
        };
    }

    /// <summary>
    ///     Finds the Halo Wars DE game directory.
    /// </summary>
    public static string? FindHaloWarsGameDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 459220");

                if (key?.GetValue("InstallLocation") is string installPath && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    /// <summary>
    ///     Finds the Halo Wars 2 game directory.
    /// </summary>
    public static string? FindHaloWars2GameDirectory()
    {
        // TODO: Implement HW2 detection (Steam, Windows Store, etc.)
        return null;
    }

    public static string? FindSoundTableXml(string gameDir)
    {
        return Directory.GetFiles(gameDir, "soundtable.xml", SearchOption.AllDirectories).FirstOrDefault();
    }

    public static void EnsureDirectoryCreated(string path)
    {
        path = Path.GetDirectoryName(Path.GetFullPath(path))!;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    ///     Result of resolving input files from settings.
    /// </summary>
    /// <param name="Files">List of absolute file paths to process.</param>
    /// <param name="Game">The resolved game, or null if using direct file.</param>
    /// <param name="GameDir">The resolved game directory, or null if using direct file.</param>
    /// <param name="Error">Error message if resolution failed, null otherwise.</param>
    public record FileResolutionResult(
        List<string> Files,
        SupportedGame? Game,
        string? GameDir,
        string? Error)
    {
        public bool Success => Error is null && Files.Count > 0;
    }

    /// <summary>
    ///     Result of resolving game and directory from settings.
    /// </summary>
    /// <param name="Game">The resolved game, or Unknown if not found.</param>
    /// <param name="GameDir">The resolved game directory, or null if not found.</param>
    /// <param name="Metadata">The game metadata, or null if game not supported.</param>
    public record GameResolutionResult(SupportedGame Game, string? GameDir, GameMetadata? Metadata);
}
