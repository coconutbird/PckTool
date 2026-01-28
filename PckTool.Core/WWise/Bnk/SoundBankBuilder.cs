using PckTool.Abstractions;
using PckTool.Core.WWise.Bnk.Structs;

namespace PckTool.Core.WWise.Bnk;

/// <summary>
///     Builder for creating Wwise soundbank files from scratch.
/// </summary>
/// <remarks>
///     Use this builder to construct new soundbanks with custom content.
///     The builder uses a fluent API pattern for ease of use.
/// </remarks>
/// <example>
///     <code>
/// var soundBank = new SoundBankBuilder()
///     .WithId(12345678)
///     .WithVersion(0x71)
///     .WithLanguage(0)
///     .AddMedia(sourceId, wemData)
///     .Build();
/// </code>
/// </example>
public class SoundBankBuilder : ISoundBankBuilder
{
    private readonly List<HircItem> _hircItems = [];
    private readonly List<(uint sourceId, byte[] data)> _mediaEntries = [];
    private uint _id;
    private uint _languageId;
    private uint _version = 0x71; // Default Wwise version

    /// <inheritdoc />
    public ISoundBankBuilder WithId(uint bankId)
    {
        _id = bankId;

        return this;
    }

    /// <inheritdoc />
    public ISoundBankBuilder WithVersion(uint version)
    {
        _version = version;

        return this;
    }

    /// <inheritdoc />
    public ISoundBankBuilder WithLanguage(uint languageId)
    {
        _languageId = languageId;

        return this;
    }

    /// <inheritdoc />
    public ISoundBankBuilder AddMedia(uint sourceId, byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _mediaEntries.Add((sourceId, data));

        return this;
    }

    /// <inheritdoc />
    public ISoundBankBuilder AddHircItem(IHircItem hircItem)
    {
        ArgumentNullException.ThrowIfNull(hircItem);

        if (hircItem is HircItem concreteItem)
        {
            _hircItems.Add(concreteItem);
        }
        else
        {
            throw new ArgumentException(
                $"HIRC item must be of type {nameof(HircItem)} from PckTool.Core.",
                nameof(hircItem));
        }

        return this;
    }

    /// <inheritdoc />
    public ISoundBank Build()
    {
        if (_id == 0)
        {
            throw new InvalidOperationException("Bank ID must be set before building.");
        }

        var soundBank = new SoundBank { Id = _id, Version = _version, LanguageId = _languageId };

        // Add media entries
        foreach (var (sourceId, data) in _mediaEntries)
        {
            soundBank.Media.Set(sourceId, data);
        }

        // Add HIRC items
        foreach (var item in _hircItems)
        {
            soundBank.Items.Add(item);
        }

        return soundBank;
    }

    /// <inheritdoc />
    public ISoundBankBuilder Reset()
    {
        _id = 0;
        _version = 0x71;
        _languageId = 0;
        _mediaEntries.Clear();
        _hircItems.Clear();

        return this;
    }

    /// <summary>
    ///     Adds a concrete HIRC item to the soundbank.
    /// </summary>
    /// <param name="hircItem">The HIRC item to add.</param>
    /// <returns>This builder for chaining.</returns>
    public SoundBankBuilder AddHircItem(HircItem hircItem)
    {
        ArgumentNullException.ThrowIfNull(hircItem);
        _hircItems.Add(hircItem);

        return this;
    }
}
