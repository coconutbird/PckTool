namespace PckTool.Core.WWise.Bnk.Enums;

/// <summary>
///     Wwise language IDs for bank versions ≤122.
///     For versions >122, language is stored as an FNV hash of the language string.
/// </summary>
public enum LegacyLanguageId : uint
{
    /// <summary>
    ///     Sound effects (non-localized). This is the default.
    /// </summary>
    Sfx = 0x00,

    Arabic = 0x01,
    Bulgarian = 0x02,
    ChineseHK = 0x03,
    ChinesePRC = 0x04,
    ChineseTaiwan = 0x05,
    Czech = 0x06,
    Danish = 0x07,
    Dutch = 0x08,
    EnglishAustralia = 0x09,
    EnglishIndia = 0x0A,
    EnglishUK = 0x0B,
    EnglishUS = 0x0C,
    Finnish = 0x0D,
    FrenchCanada = 0x0E,
    FrenchFrance = 0x0F,
    German = 0x10,
    Greek = 0x11,
    Hebrew = 0x12,
    Hungarian = 0x13,
    Indonesian = 0x14,
    Italian = 0x15,
    Japanese = 0x16,
    Korean = 0x17,
    Latin = 0x18,
    Norwegian = 0x19,
    Polish = 0x1A,
    PortugueseBrazil = 0x1B,
    PortuguesePortugal = 0x1C,
    Romanian = 0x1D,
    Russian = 0x1E,
    Slovenian = 0x1F,
    SpanishMexico = 0x20,
    SpanishSpain = 0x21,
    SpanishUS = 0x22,
    Swedish = 0x23,
    Turkish = 0x24,
    Ukrainian = 0x25,
    Vietnamese = 0x26
}
