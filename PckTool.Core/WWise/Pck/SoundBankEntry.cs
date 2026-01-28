using PckTool.Abstractions;
using PckTool.Core.WWise.Bnk;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Represents a sound bank (.bnk) entry in a package file.
/// </summary>
public class SoundBankEntry : FileEntry<uint>, ISoundBankEntry
{
  /// <summary>
  ///     Human-readable name of the sound bank (resolved from soundtable.xml).
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  ///     The language name (resolved from LanguageMap).
  /// </summary>
  public string? Language { get; set; }

  /// <summary>
  ///     The parsed SoundBank structure (lazy-loaded on demand).
  /// </summary>
  public SoundBank? Parsed { get; set; }

  /// <inheritdoc />
  uint ISoundBankEntry.Size => (uint) base.Size;

  /// <summary>
  ///     Parses the sound bank data if not already parsed.
  /// </summary>
  public SoundBank? Parse()
  {
    if (Parsed is not null)
    {
      return Parsed;
    }

    var data = GetData();

    if (data.Length == 0)
    {
      return null;
    }

    Parsed = SoundBank.Parse(data);

    return Parsed;
  }

  /// <inheritdoc />
  ISoundBank? ISoundBankEntry.Parse() => Parse();

  /// <inheritdoc />
  void ISoundBankEntry.ReplaceWith(ISoundBank soundBank)
  {
    ReplaceWith(soundBank.ToByteArray());
    Parsed = soundBank as SoundBank;
  }

  public override string ToString()
  {
    var name = Name ?? $"0x{Id:X8}";
    var lang = Language ?? $"Lang:{LanguageId}";

    return $"{name} ({lang})";
  }
}
