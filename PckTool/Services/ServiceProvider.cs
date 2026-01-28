using PckTool.Abstractions;
using PckTool.Core.WWise.Bnk;
using PckTool.Core.WWise.Pck;

namespace PckTool.Services;

/// <summary>
///     Simple service provider for the CLI application.
///     Provides factory instances for creating and loading PCK files and soundbanks.
/// </summary>
/// <remarks>
///     This lightweight approach provides testability benefits without the overhead
///     of a full DI container for a CLI tool of this scope.
/// </remarks>
public static class ServiceProvider
{
    private static IPckFileFactory? _pckFileFactory;
    private static Func<ISoundBankBuilder>? _soundBankBuilderFactory;

    /// <summary>
    ///     Gets the PCK file factory instance.
    /// </summary>
    public static IPckFileFactory PckFileFactory => _pckFileFactory ??= new PckFileFactory();

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
    /// <param name="pckFileFactory">Custom PCK file factory.</param>
    /// <param name="soundBankBuilderFactory">Custom soundbank builder factory.</param>
    public static void Configure(
        IPckFileFactory? pckFileFactory = null,
        Func<ISoundBankBuilder>? soundBankBuilderFactory = null)
    {
        _pckFileFactory = pckFileFactory;
        _soundBankBuilderFactory = soundBankBuilderFactory;
    }

    /// <summary>
    ///     Resets the service provider to default implementations.
    /// </summary>
    public static void Reset()
    {
        _pckFileFactory = null;
        _soundBankBuilderFactory = null;
    }
}

