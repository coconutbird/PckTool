namespace PckTool.Abstractions;

/// <summary>
/// Represents a soundbank entry within a PCK file.
/// </summary>
/// <remarks>
/// This is a lightweight wrapper around soundbank data that allows lazy parsing.
/// The raw data is stored until <see cref="Parse"/> is called.
/// </remarks>
public interface ISoundBankEntry
{
    /// <summary>
    /// Gets the unique identifier for this soundbank.
    /// </summary>
    uint Id { get; }

    /// <summary>
    /// Gets the size of the soundbank data in bytes.
    /// </summary>
    uint Size { get; }

    /// <summary>
    /// Gets the language ID associated with this soundbank.
    /// </summary>
    uint LanguageId { get; }

    /// <summary>
    /// Gets the raw soundbank data.
    /// </summary>
    /// <returns>The raw BNK file data.</returns>
    byte[] GetData();

    /// <summary>
    /// Parses the soundbank data into a structured format.
    /// </summary>
    /// <returns>The parsed soundbank, or null if parsing fails.</returns>
    ISoundBank? Parse();

    /// <summary>
    /// Replaces the soundbank data with new content.
    /// </summary>
    /// <param name="data">The new soundbank data.</param>
    void ReplaceWith(byte[] data);

    /// <summary>
    /// Replaces the soundbank data with a serialized soundbank.
    /// </summary>
    /// <param name="soundBank">The soundbank to serialize and store.</param>
    void ReplaceWith(ISoundBank soundBank);
}

