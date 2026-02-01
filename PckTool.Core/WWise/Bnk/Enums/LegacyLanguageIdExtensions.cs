namespace PckTool.Core.WWise.Bnk.Enums;

/// <summary>
///     Extension methods for <see cref="LegacyLanguageId" />.
/// </summary>
public static class LegacyLanguageIdExtensions
{
    /// <summary>
    ///     Converts a legacy language ID to its display name.
    /// </summary>
    /// <param name="languageId">The language ID.</param>
    /// <returns>The display name, or null if not a valid legacy language ID.</returns>
    public static string? ToDisplayName(this LegacyLanguageId languageId)
    {
        return languageId switch
        {
            LegacyLanguageId.Sfx => "SFX",
            LegacyLanguageId.Arabic => "Arabic",
            LegacyLanguageId.Bulgarian => "Bulgarian",
            LegacyLanguageId.ChineseHK => "Chinese(HK)",
            LegacyLanguageId.ChinesePRC => "Chinese(PRC)",
            LegacyLanguageId.ChineseTaiwan => "Chinese(Taiwan)",
            LegacyLanguageId.Czech => "Czech",
            LegacyLanguageId.Danish => "Danish",
            LegacyLanguageId.Dutch => "Dutch",
            LegacyLanguageId.EnglishAustralia => "English(Australia)",
            LegacyLanguageId.EnglishIndia => "English(India)",
            LegacyLanguageId.EnglishUK => "English(UK)",
            LegacyLanguageId.EnglishUS => "English(US)",
            LegacyLanguageId.Finnish => "Finnish",
            LegacyLanguageId.FrenchCanada => "French(Canada)",
            LegacyLanguageId.FrenchFrance => "French(France)",
            LegacyLanguageId.German => "German",
            LegacyLanguageId.Greek => "Greek",
            LegacyLanguageId.Hebrew => "Hebrew",
            LegacyLanguageId.Hungarian => "Hungarian",
            LegacyLanguageId.Indonesian => "Indonesian",
            LegacyLanguageId.Italian => "Italian",
            LegacyLanguageId.Japanese => "Japanese",
            LegacyLanguageId.Korean => "Korean",
            LegacyLanguageId.Latin => "Latin",
            LegacyLanguageId.Norwegian => "Norwegian",
            LegacyLanguageId.Polish => "Polish",
            LegacyLanguageId.PortugueseBrazil => "Portuguese(Brazil)",
            LegacyLanguageId.PortuguesePortugal => "Portuguese(Portugal)",
            LegacyLanguageId.Romanian => "Romanian",
            LegacyLanguageId.Russian => "Russian",
            LegacyLanguageId.Slovenian => "Slovenian",
            LegacyLanguageId.SpanishMexico => "Spanish(Mexico)",
            LegacyLanguageId.SpanishSpain => "Spanish(Spain)",
            LegacyLanguageId.SpanishUS => "Spanish(US)",
            LegacyLanguageId.Swedish => "Swedish",
            LegacyLanguageId.Turkish => "Turkish",
            LegacyLanguageId.Ukrainian => "Ukrainian",
            LegacyLanguageId.Vietnamese => "Vietnamese",
            _ => null
        };
    }

    /// <summary>
    ///     Tries to get the display name for a language ID value.
    /// </summary>
    /// <param name="languageId">The raw language ID value.</param>
    /// <returns>The display name, or null if not a valid legacy language ID.</returns>
    public static string? GetDisplayName(uint languageId)
    {
        return ((LegacyLanguageId) languageId).ToDisplayName();
    }
}

