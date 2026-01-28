using Microsoft.Win32;

namespace PckTool.Commands;

/// <summary>
///     Helper methods for finding game files and directories.
/// </summary>
public static class GameHelpers
{
    public static string? FindHaloWarsGameDirectory()
    {
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 459220");

                if (key?.GetValue("InstallLocation") is string installPath && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }

    public static string? FindSoundTableXml(string gameDir)
    {
        return Directory.GetFiles(gameDir, "soundtable.xml", SearchOption.AllDirectories).FirstOrDefault();
    }

    public static string GetSoundsPackagePath(string gameDir)
    {
        return Path.Join(
            gameDir,
            "sound",
            "wwise_2013",
            "GeneratedSoundBanks",
            "Windows",
            "Sounds.pck");
    }

    public static void EnsureDirectoryCreated(string path)
    {
        path = Path.GetDirectoryName(Path.GetFullPath(path))!;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    ///     Resolves the game directory from settings or auto-detection.
    /// </summary>
    public static string? ResolveGameDirectory(string? gameDirArg)
    {
        return gameDirArg ?? FindHaloWarsGameDirectory();
    }
}
