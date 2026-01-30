namespace PckTool.Core.Games;

/// <summary>
///     Enumeration of supported games.
/// </summary>
public enum SupportedGame
{
    /// <summary>
    ///     Unknown or unsupported game.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Halo Wars: Definitive Edition.
    /// </summary>
    HaloWars,

    /// <summary>
    ///     Halo Wars 2 (Steam/Windows Store).
    /// </summary>
    HaloWars2
}

/// <summary>
///     Extension methods for <see cref="SupportedGame" />.
/// </summary>
public static class SupportedGameExtensions
{
    /// <summary>
    ///     Parses a string to a <see cref="SupportedGame" /> enum value.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed game, or <see cref="SupportedGame.Unknown" /> if not recognized.</returns>
    public static SupportedGame ParseGame(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SupportedGame.Unknown;
        }

        return value.ToLowerInvariant() switch
        {
            "hw2" or "halowars2" or "halo-wars-2" => SupportedGame.HaloWars2,
            "hwde" or "halowarsde" or "halo-wars-de" => SupportedGame.HaloWars,
            _ => SupportedGame.Unknown
        };
    }

    /// <summary>
    ///     Gets the canonical string identifier for a game.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <returns>The canonical string identifier.</returns>
    public static string ToGameId(this SupportedGame game)
    {
        return game switch
        {
            SupportedGame.HaloWars2 => "hw2",
            SupportedGame.HaloWars => "hwde",
            _ => "unknown"
        };
    }

    /// <summary>
    ///     Gets the display name for a game.
    /// </summary>
    /// <param name="game">The game.</param>
    /// <returns>The display name.</returns>
    public static string ToDisplayName(this SupportedGame game)
    {
        return game switch
        {
            SupportedGame.HaloWars2 => "Halo Wars 2",
            SupportedGame.HaloWars => "Halo Wars: Definitive Edition",
            _ => "Unknown"
        };
    }

    /// <summary>
    ///     Gets all valid game identifiers that can be used in project files.
    /// </summary>
    /// <returns>Array of valid game identifiers.</returns>
    public static string[] GetValidGameIds()
    {
        return ["hw2", "hwde"];
    }
}
