using System.Text;

using Microsoft.Win32;

using SoundsUnpack.WWise;
using SoundsUnpack.WWise.Enums;

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

            var soundTable = new SoundTable();

            if (!soundTable.Load("C:\\Users\\dev\\Downloads\\soundtable.xml"))
            {
                return;
            }

            if (soundbank.HircChunk is not null)
            {
                var items = soundbank.HircChunk.LoadedItems;

                if (items is not null)
                {
                    foreach (var item in items)
                    {
                        if (item.Type != HircType.Event)
                        {
                            continue;
                        }

                        var sound = soundTable.GetSoundByCueIndex(item.Id);

                        var soundItem = soundbank.HircChunk.ResolveSoundFileIds(soundbank, item).ToArray();

                        sound?.FileIds.AddRange(soundItem);
                    }
                }
            }

            var path = Path.Join("dumps", language);

            EnsureDirectoryCreated(path);

            var bnkFile = Path.Join(path, $"{fileEntry.FileId:X8}.bnk");

            EnsureDirectoryCreated(bnkFile);

            // I need a counter with a string index
            var usedFiles = new Dictionary<string, int>();
            foreach (var wem in soundbank.DataChunk?.Data ?? [])
            {
                if (!wem.IsValid)
                {
                    Console.WriteLine("  Invalid WEM data!");

                    continue;
                }

                var soundFile = soundTable.GetSoundByFileId(wem.Id);
                var wemFileName = $"{wem.Id}";

                if (soundFile?.CueName is not null)
                {
                    wemFileName += $"_{soundFile.CueName}";

                    if (!usedFiles.ContainsKey(soundFile.CueName))
                    {
                        usedFiles[soundFile.CueName] = 0;
                    }

                    wemFileName += $"_{usedFiles[soundFile.CueName]++}";
                }

                var wemFile = Path.Join(path, $"{soundbank.SoundbankId:X8}", $"{wemFileName}.wem");

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
