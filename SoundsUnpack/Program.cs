using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;

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
    private static void EnsureDirectoryCreated(string path)
    {
        path = Path.GetDirectoryName(Path.GetFullPath(path))!;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void Main(string[] args)
    {
        var package = new FilePackage(
            "C:\\Program Files (x86)\\Steam\\steamapps\\common\\HaloWarsDE\\sound\\wwise_2013\\GeneratedSoundBanks\\Windows\\Sounds.pck");

        if (!package.Load())
        {
            Console.WriteLine("Failed to find sounds file");

            return;
        }

        var b = package.SoundBanksLut.Entries.FirstOrDefault(x => x.FileId == Hash.GetIdFromString("init.bnk"));

        if (b != null)
        {
            Console.WriteLine("Found init.bnk in package!");
        }

        var failed = 1;

        foreach (var soundbank in package.SoundBanksLut.Entries)
        {
            // /if (soundbank.FileId != 0x51BF5ACF)
            // /{
            // /    continue;
            // /}

            var language = package.LanguageMap[soundbank.LanguageId];

            Console.WriteLine(
                $"Soundbank ID: {soundbank.FileId:X8} Language: {language} Size: {soundbank.FileSize} bytes");

            var parser = new SoundBank();

            if (!parser.Read(new BinaryReader(new MemoryStream(soundbank.Data))))
            {
                Console.WriteLine("  Failed to parse soundbank: " + failed++);

                continue;
            }

            var path = Path.Join("dumps", language);

            EnsureDirectoryCreated(path);

            var bnkFile = Path.Join(path, $"{soundbank.FileId:x8}.bnk");

            EnsureDirectoryCreated(bnkFile);

            foreach (var wem in parser.DataChunk?.Data ?? [])
            {
                var wemFile = Path.Join(path, $"{parser.SoundbankId:X8}", $"{wem.Id:X8}.wem");

                if (wem.Id == 2447981426)
                {
                    Console.WriteLine("Debug");
                }

                EnsureDirectoryCreated(wemFile);

                File.WriteAllBytes(wemFile, wem.Data);
            }

            File.WriteAllBytes(bnkFile, soundbank.Data);
        }

        // SoundBanks.AddRange(soundBanks.Entries.Select(bankEntry =>
        // {
        //     var soundbank = new SoundBank();
        //     return soundbank.Read(new BinaryReader(new MemoryStream(bankEntry.Data))) ? soundbank : null;
        // }).Where(sb => sb != null)!);

        // File.WriteAllBytes("./dump.bin", SoundBanks[0].Wems[0].Data);
        // 
        // var hash = Hash.Fnv1A32("init.bnk");
        // var file = package.SoundBanks.FirstOrDefault(x => x.SoundbankId == hash);

        Console.WriteLine("Done!");
    }
}