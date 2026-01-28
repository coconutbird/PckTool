namespace PckTool.Abstractions;

/// <summary>
///     Factory for creating and loading Wwise PCK package files.
/// </summary>
public interface IPckFileFactory
{
    /// <summary>
    ///     Creates a new empty PCK file.
    /// </summary>
    /// <returns>A new empty PCK file instance.</returns>
    IPckFile Create();

    /// <summary>
    ///     Loads a PCK file from the specified path.
    /// </summary>
    /// <param name="path">The path to the PCK file.</param>
    /// <returns>The loaded PCK file.</returns>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidDataException">The file is not a valid PCK file.</exception>
    IPckFile Load(string path);

    /// <summary>
    ///     Loads a PCK file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing PCK data.</param>
    /// <returns>The loaded PCK file.</returns>
    /// <exception cref="InvalidDataException">The stream does not contain valid PCK data.</exception>
    IPckFile Load(Stream stream);
}
