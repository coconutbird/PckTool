using PckTool.Abstractions;

namespace PckTool.Core.WWise.Pck;

/// <summary>
///     Factory for creating and loading Wwise PCK package files.
/// </summary>
public class PckFileFactory : IPckFileFactory
{
    /// <summary>
    ///     Creates a new empty PCK file.
    /// </summary>
    /// <returns>A new empty PCK file instance.</returns>
    public IPckFile Create()
    {
        return PckFile.Create();
    }

    /// <summary>
    ///     Loads a PCK file from the specified path.
    /// </summary>
    /// <param name="path">The path to the PCK file.</param>
    /// <returns>The loaded PCK file.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the file is not a valid PCK file.</exception>
    public IPckFile Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("PCK file not found.", path);
        }

        var pckFile = PckFile.Load(path);

        if (pckFile is null)
        {
            throw new InvalidDataException($"Failed to load PCK file: {path}");
        }

        return pckFile;
    }

    /// <summary>
    ///     Loads a PCK file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing PCK data.</param>
    /// <returns>The loaded PCK file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    /// <exception cref="InvalidDataException">Thrown when the stream does not contain valid PCK data.</exception>
    public IPckFile Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var pckFile = PckFile.Load(stream);

        if (pckFile is null)
        {
            throw new InvalidDataException("Failed to load PCK file from stream.");
        }

        return pckFile;
    }
}
