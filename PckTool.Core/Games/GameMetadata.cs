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
    ///     Gets whether this game requires the user to specify the game directory manually.
    /// </summary>
    public virtual bool RequiresManualGameDirectory => false;

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
            SupportedGame.HaloWars2 => HaloWars2Metadata.Instance,
            _ => null
        };
    }
}

/// <summary>
///     Metadata for Halo Wars: Definitive Edition.
/// </summary>
public class HaloWarsMetadata : GameMetadata
{
    private HaloWarsMetadata()
    {
        Game = SupportedGame.HaloWars;
    }

    /// <summary>
    ///     Singleton instance.
    /// </summary>
    public static HaloWarsMetadata Instance { get; } = new();

    /// <summary>
    ///     Relative path to the Sounds.pck file from the game directory.
    /// </summary>
    public static string SoundsPackageRelativePath =>
        Path.Combine("sound", "wwise_2013", "GeneratedSoundBanks", "Windows", "Sounds.pck");

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

/// <summary>
///     Metadata for Halo Wars 2.
///     Since HW2 is a UWP game, the game directory must always be specified manually.
///     This class scans for all .bnk and .pck files in the game directory.
/// </summary>
public class HaloWars2Metadata : GameMetadata
{
    private HaloWars2Metadata()
    {
        Game = SupportedGame.HaloWars2;
    }

    /// <summary>
    ///     Singleton instance.
    /// </summary>
    public static HaloWars2Metadata Instance { get; } = new();

    /// <summary>
    ///     Gets whether this game requires the user to specify the game directory manually.
    ///     Always true for HW2 since it's a UWP game that cannot be auto-detected.
    /// </summary>
    public override bool RequiresManualGameDirectory => true;

    /// <inheritdoc />
    /// <remarks>
    ///     Scans the game directory recursively for all .bnk and .pck files.
    /// </remarks>
    public override IEnumerable<string> GetDefaultInputFiles(string gameDirectory)
    {
        if (!Directory.Exists(gameDirectory))
        {
            yield break;
        }

        // Scan for all .pck files
        foreach (var pckFile in Directory.EnumerateFiles(gameDirectory, "*.pck", SearchOption.AllDirectories))
        {
            yield return Path.GetRelativePath(gameDirectory, pckFile);
        }

        // Scan for all .bnk files
        foreach (var bnkFile in Directory.EnumerateFiles(gameDirectory, "*.bnk", SearchOption.AllDirectories))
        {
            yield return Path.GetRelativePath(gameDirectory, bnkFile);
        }
    }
}
