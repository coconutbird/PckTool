namespace SoundsUnpack.WWise;


public class FilePackage(string fileName) : IDisposable
{
    public Dictionary<uint, string> LanguageMap { get; private set; } = new();
    public FilePackageLut<uint> SoundBanksLut { get; private set; } = new();
    public FilePackageLut<uint> StmFilesLut { get; private set; } = new();
    public FilePackageLut<ulong> ExternalLuts { get; private set; } = new();

    // public List<SoundBank> SoundBanks { get; } = new();

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

    public void Dispose()
    {
        _reader.Dispose();
    }

    private const uint ValidVersion = 0x1;

    private readonly uint _validHeaderTag = Hash.AkmmioFourcc('A', 'K', 'P', 'K');
    private readonly BinaryReader _reader = new(File.OpenRead(fileName));
}