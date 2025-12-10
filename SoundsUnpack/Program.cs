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

        foreach (var fileEntry in package.SoundBanksLut.Entries)
        {
            var language = package.LanguageMap[fileEntry.LanguageId];

            Console.WriteLine(
                $"Soundbank ID: {fileEntry.FileId:X8} Language: {language} Size: {fileEntry.FileSize} bytes");

            var soundbank = new SoundBank();

            if (!soundbank.Read(new BinaryReader(new MemoryStream(fileEntry.Data))))
            {
                Console.WriteLine("  Failed to parse soundbank: " + failed++);

                // well we failed to parse the whole thing, but it's still loaded enough to have the data
                if (!soundbank.IsLoaded)
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

                if (wem.Id == 2447981426)
                {
                    Console.WriteLine("Debug");
                }

                EnsureDirectoryCreated(wemFile);

                File.WriteAllBytes(wemFile, wem.Data);
            }

            File.WriteAllBytes(bnkFile, fileEntry.Data);
        }

        Console.WriteLine("Done!");
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