using System.Xml.Linq;

namespace SoundsUnpack.WWise;

public class SoundTable
{
    public bool Load(string path)
    {
        var doc = XDocument.Load(path);

        foreach (var sound in doc.Descendants("Sound"))
        {
            var cueName = sound.Element("CueName")?.Value;
            var cueIndex = sound.Element("CueIndex")?.Value;

            if (cueName is null || cueIndex is null)
            {
                continue;
            }

            var soundEntry = new Sound { CueName = cueName };

            _idSoundMap.Add(soundEntry.CueIndex, soundEntry);
        }

        return true;
    }

    public Sound? GetSoundByFileId(uint id)
    {
        return _idSoundMap.Values.FirstOrDefault(x => x.FileIds.Contains(id));
    }

    public Sound? GetSoundByCueIndex(uint id)
    {
        return _idSoundMap.GetValueOrDefault(id);
    }

    public Sound? GetSoundByName(string name)
    {
        return _idSoundMap.GetValueOrDefault(Hash.GetIdFromString(name));
    }

    public class Sound
    {
        public string CueName { get; set; } = string.Empty;
        public uint CueIndex => Hash.GetIdFromString(CueName);

        public List<uint> FileIds { get; set; } = [];
    }

    private readonly Dictionary<uint, Sound> _idSoundMap = new();
}