using System.Text;

using Microsoft.Win32;

using SoundsUnpack.WWise;

namespace SoundsUnpack;

public static class BinaryReaderExtensions
{
    public static string ReadWString(this BinaryReader reader)
    {
        var builder = new StringBuilder();

        while (true)
        {
            var buffer = reader.ReadUInt16();

            if (buffer == 0)
            {
                return builder.ToString();
            }

            builder.Append((char) buffer);
        }
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var gameDir = FindHaloWarsGameDirectory();

        if (gameDir is null)
        {
            Log.Error("Failed to find Halo Wars game directory");

            return;
        }

        var soundsPackagePath = Path.Join(
            gameDir,
            "sound",
            "wwise_2013",
            "GeneratedSoundBanks",
            "Windows",
            "Sounds.pck");

        var package = new FilePackage(soundsPackagePath);

        if (!package.Load())
        {
            Console.WriteLine("Failed to find sounds file");

            return;
        }

        var hash = Hash.GetIdFromString("init.bnk");
        var b = package.SoundBanksLut.Entries.FirstOrDefault(x => x.FileId == hash);

        if (b != null)
        {
            Console.WriteLine("Found init.bnk in package!");
        }

        var failed = 1;

        foreach (var fileEntry in package.SoundBanksLut.Entries)
        {
            var language = package.LanguageMap[fileEntry.LanguageId];

            Console.WriteLine(
                $"Soundbank ID: {fileEntry.FileId:X8} Language: {language} Size: {fileEntry.FileSize} bytes");

            var soundbank = new SoundBank();

            if (!soundbank.Read(new BinaryReader(new MemoryStream(fileEntry.Data))))
            {
                Console.WriteLine("  Failed to parse soundbank: " + failed++);

                // well we failed to parse the whole thing, but we can still extract the wems
                if (!soundbank.IsMediaLoaded)
                {
                    continue;
                }
            }

            var path = Path.Join("dumps", language);

            EnsureDirectoryCreated(path);

            var bnkFile = Path.Join(path, $"{fileEntry.FileId:x8}.bnk");

            EnsureDirectoryCreated(bnkFile);

            foreach (var wem in soundbank.DataChunk?.Data ?? [])
            {
                if (!wem.IsValid)
                {
                    Console.WriteLine("  Invalid WEM data!");

                    continue;
                }

                var wemFile = Path.Join(path, $"{soundbank.SoundbankId:X8}", $"{wem.Id:X8}.wem");

                EnsureDirectoryCreated(wemFile);

                File.WriteAllBytes(wemFile, wem.Data);
            }

            File.WriteAllBytes(bnkFile, fileEntry.Data);
        }

        Console.WriteLine("Done!");
    }

    private static string? FindHaloWarsGameDirectory()
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

    private static void EnsureDirectoryCreated(string path)
    {
        path = Path.GetDirectoryName(Path.GetFullPath(path))!;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
