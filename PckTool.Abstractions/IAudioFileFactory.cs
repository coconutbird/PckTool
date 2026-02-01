namespace PckTool.Abstractions;

/// <summary>
///     Factory for creating audio file instances from various sources.
/// </summary>
/// <remarks>
///     This factory automatically detects the file type (PCK or BNK) and returns
///     the appropriate implementation of <see cref="IAudioFile" />.
///     Directory loading is supported but must be explicitly enabled via the allowDirectories parameter.
/// </remarks>
public interface IAudioFileFactory
{
    /// <summary>
    ///     Loads an audio file from the specified path.
    ///     Automatically detects whether the path is a PCK or BNK based on extension.
    /// </summary>
    /// <param name="path">The path to the audio file.</param>
    /// <param name="allowDirectories">
    ///     If true, allows loading a directory as a composite set containing all .pck and .bnk files.
    ///     Defaults to false to prevent accidental directory loading.
    /// </param>
    /// <returns>An audio file instance (or composite set for directories when allowed).</returns>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory does not exist (when allowDirectories is true).</exception>
    /// <exception cref="InvalidDataException">The file format is not recognized, or path is a directory when not allowed.</exception>
    IAudioFile Load(string path, bool allowDirectories = false);

    /// <summary>
    ///     Loads an audio file from a stream with the specified file type.
    /// </summary>
    /// <param name="stream">The stream containing the audio file data.</param>
    /// <param name="fileType">The type of audio file.</param>
    /// <param name="sourcePath">Optional source path for reference.</param>
    /// <returns>An audio file instance.</returns>
    IAudioFile Load(Stream stream, AudioFileType fileType, string? sourcePath = null);

    /// <summary>
    ///     Determines the audio file type from a file path based on extension.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <returns>The detected audio file type.</returns>
    AudioFileType DetectFileType(string path);

    /// <summary>
    ///     Checks if the specified file extension is supported.
    /// </summary>
    /// <param name="extension">The file extension (with or without leading dot).</param>
    /// <returns>true if the extension is supported; otherwise, false.</returns>
    bool IsSupportedExtension(string extension);
}
