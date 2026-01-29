namespace PckTool.Core.Games;

/// <summary>
///     Base metadata for a supported game. Extend for game-specific behavior.
/// </summary>
public abstract class GameMetadata
{
    /// <summary>
    ///     Gets the game this metadata is for.
    /// </summary>
    public SupportedGame Game { get; protected init; }

    /// <summary>
    ///     Gets the default input files for this game (relative paths from game directory).
    /// </summary>
    /// <param name="gameDirectory">The game installation directory (used to verify files exist).</param>
    /// <returns>List of input file paths (relative to game directory).</returns>
    public abstract IEnumerable<string> GetDefaultInputFiles(string gameDirectory);

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

    /// <summary>
    ///     Relative path to the Sounds.pck file from the game directory.
    /// </summary>
    public static string SoundsPackageRelativePath =>
        Path.Combine("sound", "wwise_2013", "GeneratedSoundBanks", "Windows", "Sounds.pck");

    private HaloWarsMetadata()
    {
        Game = SupportedGame.HaloWars;
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetDefaultInputFiles(string gameDirectory)
    {
        var absolutePath = Path.Combine(gameDirectory, SoundsPackageRelativePath);

        if (File.Exists(absolutePath))
        {
            yield return SoundsPackageRelativePath;
        }
    }
}
