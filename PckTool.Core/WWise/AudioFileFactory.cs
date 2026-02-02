using PckTool.Abstractions;
using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Pck;

namespace PckTool.Core.WWise;

/// <summary>
///     Factory for creating audio file instances from various sources.
/// </summary>
/// <remarks>
///     This factory automatically detects the file type (PCK or BNK) based on
///     the file extension and returns the appropriate implementation of <see cref="IAudioFile" />.
/// </remarks>
public class AudioFileFactory : IAudioFileFactory
{
    private static readonly HashSet<string> PckExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pck" };
    private static readonly HashSet<string> BnkExtensions = new(StringComparer.OrdinalIgnoreCase) { ".bnk" };

    /// <inheritdoc />
    public IAudioFile Load(string path, bool allowDirectories = false)
    {
        var fileType = DetectFileType(path, allowDirectories);

        // For composite (directory), check directory exists; for files, check file exists
        if (fileType == AudioFileType.Composite)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }
        }
        else
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Audio file not found.", path);
            }
        }

        return fileType switch
        {
            AudioFileType.Pck => LoadPck(path),
            AudioFileType.Bnk => LoadBnk(path),
            AudioFileType.Composite => LoadComposite(path),
            _ => throw new InvalidDataException($"Unknown audio file type: {path}")
        };
    }

    /// <inheritdoc />
    public IAudioFile Load(Stream stream, AudioFileType fileType, string? sourcePath = null)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return fileType switch
        {
            AudioFileType.Pck => LoadPckFromStream(stream, sourcePath),
            AudioFileType.Bnk => LoadBnkFromStream(stream, sourcePath),
            AudioFileType.Composite => throw new ArgumentException(
                "Cannot load a composite audio file from a stream.",
                nameof(fileType)),
            _ => throw new ArgumentException($"Unknown audio file type: {fileType}", nameof(fileType))
        };
    }

    /// <inheritdoc />
    public AudioFileType DetectFileType(string path)
    {
        return DetectFileType(path, false);
    }

    /// <inheritdoc />
    public bool IsSupportedExtension(string extension)
    {
        // Normalize extension to have a leading dot
        if (!extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        return PckExtensions.Contains(extension) || BnkExtensions.Contains(extension);
    }

    /// <summary>
    ///     Determines the audio file type from a file path based on extension.
    /// </summary>
    /// <param name="path">The file path to check.</param>
    /// <param name="allowDirectories">If true, allows detecting directories as composite type.</param>
    /// <returns>The detected audio file type.</returns>
    /// <exception cref="InvalidDataException">
    ///     Thrown when the extension is not supported, or when path is a directory and allowDirectories is false.
    /// </exception>
    public AudioFileType DetectFileType(string path, bool allowDirectories)
    {
        var extension = Path.GetExtension(path);

        if (PckExtensions.Contains(extension))
        {
            return AudioFileType.Pck;
        }

        if (BnkExtensions.Contains(extension))
        {
            return AudioFileType.Bnk;
        }

        // Check if path is a directory (with or without trailing separator)
        if (Path.EndsInDirectorySeparator(path) || Directory.Exists(path))
        {
            if (!allowDirectories)
            {
                throw new InvalidDataException(
                    $"Path appears to be a directory. Set allowDirectories=true to load directories as composite sets: {path}");
            }

            return AudioFileType.Composite;
        }

        throw new InvalidDataException($"Unsupported audio file extension: {extension}");
    }

    private static IAudioFile LoadPck(string path)
    {
        var pckFile = PckFile.Load(path);

        if (pckFile is null)
        {
            throw new InvalidDataException($"Failed to load PCK file: {path}");
        }

        return pckFile;
    }

    private static IAudioFile LoadBnk(string path)
    {
        return BnkFile.Load(path);
    }

    private static IAudioFile LoadPckFromStream(Stream stream, string? sourcePath)
    {
        var pckFile = PckFile.Load(stream, sourcePath);

        if (pckFile is null)
        {
            throw new InvalidDataException("Failed to load PCK file from stream.");
        }

        return pckFile;
    }

    private static IAudioFile LoadBnkFromStream(Stream stream, string? sourcePath)
    {
        return BnkFile.Load(stream, sourcePath);
    }

    private IAudioFile LoadComposite(string path)
    {
        // only .bnk and .pck files
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                             .Where(f => IsSupportedExtension(Path.GetExtension(f)))
                             .Select(f => Load(f))
                             .ToList();

        return new AudioFileSet.AudioFileSet(files);
    }
}
