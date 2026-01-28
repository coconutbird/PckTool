namespace PckTool.Abstractions;

/// <summary>
/// Builder for creating Wwise soundbank files from scratch.
/// </summary>
/// <remarks>
/// Use this builder to construct new soundbanks with custom content.
/// The builder uses a fluent API pattern for ease of use.
/// </remarks>
/// <example>
/// <code>
/// var soundBank = builder
///     .WithId(12345678)
///     .WithVersion(134)
///     .WithLanguage(0)
///     .AddMedia(sourceId, wemData)
///     .Build();
/// </code>
/// </example>
public interface ISoundBankBuilder
{
    /// <summary>
    /// Sets the soundbank's unique identifier.
    /// </summary>
    /// <param name="bankId">The bank ID (typically a FNV hash of the bank name).</param>
    /// <returns>This builder for chaining.</returns>
    ISoundBankBuilder WithId(uint bankId);

    /// <summary>
    /// Sets the Wwise version for the soundbank.
    /// </summary>
    /// <param name="version">The Wwise version number.</param>
    /// <returns>This builder for chaining.</returns>
    ISoundBankBuilder WithVersion(uint version);

    /// <summary>
    /// Sets the language ID for localized soundbanks.
    /// </summary>
    /// <param name="languageId">The language ID (0 for SFX/non-localized).</param>
    /// <returns>This builder for chaining.</returns>
    ISoundBankBuilder WithLanguage(uint languageId);

    /// <summary>
    /// Adds embedded media to the soundbank.
    /// </summary>
    /// <param name="sourceId">The source ID for the WEM file.</param>
    /// <param name="data">The WEM file data.</param>
    /// <returns>This builder for chaining.</returns>
    ISoundBankBuilder AddMedia(uint sourceId, byte[] data);

    /// <summary>
    /// Adds a HIRC item to the soundbank.
    /// </summary>
    /// <param name="hircItem">The HIRC item to add.</param>
    /// <returns>This builder for chaining.</returns>
    ISoundBankBuilder AddHircItem(IHircItem hircItem);

    /// <summary>
    /// Builds the soundbank with all configured settings.
    /// </summary>
    /// <returns>The constructed soundbank.</returns>
    /// <exception cref="InvalidOperationException">Required properties were not set.</exception>
    ISoundBank Build();

    /// <summary>
    /// Resets the builder to its initial state.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    ISoundBankBuilder Reset();
}

