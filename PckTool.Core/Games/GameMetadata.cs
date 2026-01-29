namespace PckTool.Core.Games;

/// <summary>
///     Base metadata for a supported game. Extend for game-specific behavior.
/// </summary>
public class GameMetadata
{
    /// <summary>
    ///     Gets the game this metadata is for.
    /// </summary>
    public SupportedGame Game { get; protected init; }

    /// <summary>
    ///     Gets the metadata for a specific game.
    /// </summary>
    /// <param name="game">The game to get metadata for.</param>
    /// <returns>The game metadata, or null if not supported.</returns>
    public static GameMetadata? GetMetadata(SupportedGame game)
    {
        return game switch
        {
            SupportedGame.HaloWars => HaloWarsMetadata.Instance,
            _ => null
        };
    }
}

/// <summary>
///     Metadata for Halo Wars: Definitive Edition.
/// </summary>
public class HaloWarsMetadata : GameMetadata
{
    /// <summary>
    ///     Singleton instance.
    /// </summary>
    public static HaloWarsMetadata Instance { get; } = new();

    private HaloWarsMetadata()
    {
        Game = SupportedGame.HaloWars;
    }

    /// <summary>
    ///     Gets the relative path to the Sounds.pck file.
    /// </summary>
    public string SoundsPackageRelativePath =>
        Path.Combine("sound", "wwise_2013", "GeneratedSoundBanks", "Windows", "Sounds.pck");

    /// <summary>
    ///     Gets the full path to Sounds.pck given a game directory.
    /// </summary>
    public string GetSoundsPackagePath(string gameDirectory)
    {
        return Path.Combine(gameDirectory, SoundsPackageRelativePath);
    }

    /// <summary>
    ///     Finds the sound table XML file in the game directory.
    /// </summary>
    public string? FindSoundTablePath(string gameDirectory)
    {
        try
        {
            return Directory.GetFiles(gameDirectory, "soundtable.xml", SearchOption.AllDirectories)
                            .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}
