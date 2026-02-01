namespace PckTool.Abstractions;

/// <summary>
///     Specifies the type of audio file.
/// </summary>
public enum AudioFileType
{
    /// <summary>
    ///     A PCK package file containing multiple soundbanks and streaming files.
    /// </summary>
    Pck,

    /// <summary>
    ///     A standalone BNK soundbank file.
    /// </summary>
    Bnk
}

/// <summary>
///     Represents a unified audio file that can be either a PCK package or a standalone BNK soundbank.
/// </summary>
/// <remarks>
///     This interface provides a common abstraction for working with both:
///     <list type="bullet">
///         <item>
///             <description>PCK files - containers with multiple soundbanks and streaming files</description>
///         </item>
///         <item>
///             <description>BNK files - standalone soundbanks with embedded media</description>
///         </item>
///     </list>
/// </remarks>
public interface IAudioFile : IDisposable
{
    /// <summary>
    ///     Gets the source file path this audio file was loaded from, if applicable.
    /// </summary>
    string? SourcePath { get; }

    /// <summary>
    ///     Gets whether any entries have been modified since loading.
    /// </summary>
    bool HasModifications { get; }

    /// <summary>
    ///     Gets the type of audio file (PCK or BNK).
    /// </summary>
    AudioFileType FileType { get; }

    /// <summary>
    ///     Gets the soundbank entries in this audio file.
    ///     For BNK files, this returns a single-entry collection.
    /// </summary>
    ISoundBankCollection SoundBanks { get; }

    /// <summary>
    ///     Gets the streaming file entries in this audio file.
    ///     For BNK files, this returns an empty collection.
    /// </summary>
    IStreamingFileCollection StreamingFiles { get; }

    /// <summary>
    ///     Gets the external file entries in this audio file.
    ///     For BNK files, this returns an empty collection.
    /// </summary>
    IExternalFileCollection ExternalFiles { get; }

    /// <summary>
    ///     Gets the language ID to name mapping.
    ///     For BNK files, this returns a single-entry mapping.
    /// </summary>
    IReadOnlyDictionary<uint, string> Languages { get; }

    /// <summary>
    ///     Gets the number of soundbanks contained in this audio file.
    /// </summary>
    int SoundBankCount { get; }

    /// <summary>
    ///     Gets the number of streaming WEM files in this audio file.
    /// </summary>
    int StreamingFileCount { get; }

    /// <summary>
    ///     Gets the number of external WEM files in this audio file.
    /// </summary>
    int ExternalFileCount { get; }

    /// <summary>
    ///     Finds a WEM file by its source ID across all storage locations.
    /// </summary>
    /// <param name="sourceId">The Wwise source ID of the WEM file.</param>
    /// <returns>The WEM data if found; otherwise, null.</returns>
    byte[]? FindWem(uint sourceId);

    /// <summary>
    ///     Determines whether a WEM with the specified source ID exists.
    /// </summary>
    /// <param name="sourceId">The Wwise source ID to search for.</param>
    /// <returns>true if the WEM exists; otherwise, false.</returns>
    bool ContainsWem(uint sourceId);

    /// <summary>
    ///     Replaces a WEM file's data across all locations where it exists.
    /// </summary>
    /// <param name="sourceId">The source ID of the WEM to replace.</param>
    /// <param name="data">The new WEM data.</param>
    /// <param name="updateHircSizes">Whether to update HIRC size references.</param>
    /// <returns>A result describing what was replaced.</returns>
    WemReplacementResult ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true);

    /// <summary>
    ///     Saves the audio file to the specified path.
    /// </summary>
    /// <param name="path">The file path to save to.</param>
    void Save(string path);

    /// <summary>
    ///     Saves the audio file to a stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    void Save(Stream stream);
}

public static class AudioFileExtensions
{
    /// <summary>
    ///     Gets the language name for a language ID, or a default formatted name if not found.
    /// </summary>
    /// <param name="audioFile">The audio file.</param>
    /// <param name="languageId">The language ID.</param>
    /// <returns>The language name, or a default formatted name if not found.</returns>
    public static string GetLanguageNameOrDefault(this IAudioFile audioFile, uint languageId)
    {
        // First check if we have a mapping in the Languages dictionary
        var result = audioFile.Languages.GetValueOrDefault(languageId);

        // Fall back to legacy language ID enum for versions <= 122
        if (string.IsNullOrEmpty(result))
        {
            result = (LegacyLanguageId) languageId switch
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
                _ => $"Lang_{languageId}"
            };
        }

        return result;
    }

    /// <summary>
    ///     Wwise language IDs for bank versions ≤122.
    ///     For versions >122, language is stored as an FNV hash of the language string.
    /// </summary>
    private enum LegacyLanguageId : uint
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
}