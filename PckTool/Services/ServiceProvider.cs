using PckTool.Abstractions;
using PckTool.Core.WWise;
using PckTool.Core.WWise.Bnk;

namespace PckTool.Services;

/// <summary>
///     Simple service provider for the CLI application.
///     Provides factory instances for creating and loading audio files (PCK and BNK) and soundbanks.
/// </summary>
/// <remarks>
///     This lightweight approach provides testability benefits without the overhead
///     of a full DI container for a CLI tool of this scope.
/// </remarks>
public static class ServiceProvider
{
    private static IAudioFileFactory? _audioFileFactory;
    private static Func<ISoundBankBuilder>? _soundBankBuilderFactory;

    /// <summary>
    ///     Gets the audio file factory instance.
    ///     This factory can load both PCK and BNK files.
    /// </summary>
    public static IAudioFileFactory AudioFileFactory => _audioFileFactory ??= new AudioFileFactory();

    /// <summary>
    ///     Creates a new soundbank builder instance.
    /// </summary>
    public static ISoundBankBuilder CreateSoundBankBuilder()
    {
        return _soundBankBuilderFactory?.Invoke() ?? new SoundBankBuilder();
    }

    /// <summary>
    ///     Configures the service provider with custom implementations.
    ///     Useful for testing or alternative implementations.
    /// </summary>
    /// <param name="audioFileFactory">Custom audio file factory.</param>
    /// <param name="soundBankBuilderFactory">Custom soundbank builder factory.</param>
    public static void Configure(
        IAudioFileFactory? audioFileFactory = null,
        Func<ISoundBankBuilder>? soundBankBuilderFactory = null)
    {
        _audioFileFactory = audioFileFactory;
        _soundBankBuilderFactory = soundBankBuilderFactory;
    }

    /// <summary>
    ///     Resets the service provider to default implementations.
    /// </summary>
    public static void Reset()
    {
        _audioFileFactory = null;
        _soundBankBuilderFactory = null;
    }
}
