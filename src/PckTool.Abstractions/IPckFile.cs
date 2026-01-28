namespace PckTool.Abstractions;

/// <summary>
/// Represents a Wwise PCK package file containing soundbanks and streaming audio.
/// </summary>
/// <remarks>
/// PCK files are the primary container format used by Wwise for packaging audio assets.
/// They contain soundbanks (BNK files) and optionally streaming WEM files.
/// </remarks>
public interface IPckFile : IDisposable
{
  /// <summary>
  /// Gets the number of soundbanks contained in this package.
  /// </summary>
  int SoundBankCount { get; }

  /// <summary>
  /// Gets the number of streaming WEM files in this package.
  /// </summary>
  int StreamingFileCount { get; }

  /// <summary>
  /// Finds a WEM file by its source ID across all storage locations.
  /// </summary>
  /// <param name="sourceId">The Wwise source ID of the WEM file.</param>
  /// <returns>The WEM data if found; otherwise, null.</returns>
  byte[]? FindWem(uint sourceId);

  /// <summary>
  /// Determines whether a WEM with the specified source ID exists.
  /// </summary>
  /// <param name="sourceId">The Wwise source ID to search for.</param>
  /// <returns>true if the WEM exists; otherwise, false.</returns>
  bool ContainsWem(uint sourceId);

  /// <summary>
  /// Adds a soundbank to this package.
  /// </summary>
  /// <param name="soundBank">The soundbank to add.</param>
  void AddSoundBank(ISoundBank soundBank);

  /// <summary>
  /// Removes a soundbank from this package.
  /// </summary>
  /// <param name="bankId">The ID of the soundbank to remove.</param>
  /// <returns>true if the soundbank was removed; otherwise, false.</returns>
  bool RemoveSoundBank(uint bankId);

  /// <summary>
  /// Adds a streaming WEM file to this package.
  /// </summary>
  /// <param name="sourceId">The Wwise source ID for the WEM.</param>
  /// <param name="data">The WEM file data.</param>
  void AddStreamingFile(uint sourceId, byte[] data);

  /// <summary>
  /// Removes a streaming WEM file from this package.
  /// </summary>
  /// <param name="sourceId">The source ID of the WEM to remove.</param>
  /// <returns>true if the WEM was removed; otherwise, false.</returns>
  bool RemoveStreamingFile(uint sourceId);

  /// <summary>
  /// Replaces a WEM file's data across all locations where it exists.
  /// </summary>
  /// <param name="sourceId">The source ID of the WEM to replace.</param>
  /// <param name="data">The new WEM data.</param>
  /// <param name="updateHircSizes">Whether to update HIRC size references.</param>
  /// <returns>A result describing what was replaced.</returns>
  WemReplacementResult ReplaceWem(uint sourceId, byte[] data, bool updateHircSizes = true);

  /// <summary>
  /// Saves the package to the specified path.
  /// </summary>
  /// <param name="path">The file path to save to.</param>
  void Save(string path);

  /// <summary>
  /// Saves the package to a stream.
  /// </summary>
  /// <param name="stream">The stream to write to.</param>
  void Save(Stream stream);
}

