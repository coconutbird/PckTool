using Microsoft.Win32;

using PckTool.Core.Games;

namespace PckTool.Commands;

/// <summary>
///     Helper methods for finding game files and directories.
/// </summary>
public static class GameHelpers
{
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
    ///     Result of resolving game and directory from settings.
    /// </summary>
    /// <param name="Game">The resolved game, or Unknown if not found.</param>
    /// <param name="GameDir">The resolved game directory, or null if not found.</param>
    /// <param name="Metadata">The game metadata, or null if game not supported.</param>
    public record GameResolutionResult(SupportedGame Game, string? GameDir, GameMetadata? Metadata);
}
