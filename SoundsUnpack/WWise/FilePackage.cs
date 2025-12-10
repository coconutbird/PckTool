namespace SoundsUnpack.WWise;

public class FilePackage(string fileName) : IDisposable
{
    private const uint ValidVersion = 0x1;
    private readonly BinaryReader _reader = new(File.OpenRead(fileName));

    private readonly uint _validHeaderTag = Hash.AkmmioFourcc('A', 'K', 'P', 'K');
    public Dictionary<uint, string> LanguageMap { get; private set; } = new();
    public FilePackageLut<uint> SoundBanksLut { get; private set; } = new();
    public FilePackageLut<uint> StmFilesLut { get; private set; } = new();
    public FilePackageLut<ulong> ExternalLuts { get; private set; } = new();

    public void Dispose()
    {
        _reader.Dispose();
    }

    public bool Load()
    {
        var tag = _reader.ReadUInt32();
        var headerSize = _reader.ReadUInt32();

        if (tag != _validHeaderTag || headerSize == 0x0)
        {
            return false;
        }

        var version = _reader.ReadUInt32();

        if (version != ValidVersion)
        {
            return false;
        }

        var languageMapSize = _reader.ReadUInt32();
        var soundBanksLutSize = _reader.ReadUInt32();
        var stmFilesLutSize = _reader.ReadUInt32();
        var externalLutsSize = _reader.ReadUInt32();

        if (headerSize < 24 + languageMapSize + soundBanksLutSize + stmFilesLutSize)
        {
            return false;
        }

        var languageMap = new StringMap();

        if (!languageMap.Read(_reader, languageMapSize))
        {
            return false;
        }

        var soundBanks = new FilePackageLut<uint>();

        if (!soundBanks.Read(_reader, soundBanksLutSize))
        {
            return false;
        }

        var stmFiles = new FilePackageLut<uint>();

        if (!stmFiles.Read(_reader, stmFilesLutSize))
        {
            return false;
        }

        var externalLuts = new FilePackageLut<ulong>();

        if (!externalLuts.Read(_reader, externalLutsSize))
        {
            return false;
        }

        // expose data
        LanguageMap = languageMap.Map;
        SoundBanksLut = soundBanks;
        StmFilesLut = stmFiles;
        ExternalLuts = externalLuts;

        return true;
    }

    public void Save(string fileName)
    {
        using var writer = new BinaryWriter(File.Create(fileName));

        // write tag
        writer.Write(_validHeaderTag);

        writer.Write(0x0); // header size, will be filled in later

        writer.Write(ValidVersion);

        writer.Write(0x0); // language map size, will be filled in later
        writer.Write(0x0); // sound banks lut size, will be filled in later
        writer.Write(0x0); // stm files lut size, will be filled in later
        writer.Write(0x0); // external luts size, will be filled in later

        // write language map
        writer.Write(LanguageMap.Count);

        foreach (var (id, name) in LanguageMap)
        {
            writer.Write(name);
        }
    }
}
