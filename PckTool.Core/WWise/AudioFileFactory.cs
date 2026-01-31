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
    public IAudioFile Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Audio file not found.", path);
        }

        var fileType = DetectFileType(path);

        return fileType switch
        {
            AudioFileType.Pck => LoadPck(path),
            AudioFileType.Bnk => LoadBnk(path),
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
            _ => throw new ArgumentException($"Unknown audio file type: {fileType}", nameof(fileType))
        };
    }

    /// <inheritdoc />
    public AudioFileType DetectFileType(string path)
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

        throw new InvalidDataException($"Unsupported audio file extension: {extension}");
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
        var pckFile = PckFile.Load(stream);

        if (pckFile is null)
        {
            throw new InvalidDataException("Failed to load PCK file from stream.");
        }

        return pckFile;
    }

    private static IAudioFile LoadBnkFromStream(Stream stream, string? sourcePath)
    {
        var soundBank = SoundBank.Parse(stream);

        if (soundBank is null)
        {
            throw new InvalidDataException("Failed to load BNK file from stream.");
        }

        return new BnkFile(soundBank, sourcePath);
    }
}
