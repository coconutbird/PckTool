using System.Text;

namespace PckTool.Core.WWise.Structs;

/// <summary>
///     MusicSegment initial values for bank version 113.
///     Corresponds to CAkMusicSegment::SetInitialValues in wwiser.
/// </summary>
public class MusicSegmentInitialValues
{
    /// <summary>
    ///     Music node params.
    /// </summary>
    public MusicNodeParams MusicNodeParams { get; set; } = null!;

    /// <summary>
    ///     Duration of the segment in milliseconds.
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    ///     Markers (cue points) in the segment.
    /// </summary>
    public List<MusicMarker> Markers { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // MusicNodeParams
        var musicNodeParams = new MusicNodeParams();

        if (!musicNodeParams.Read(reader))
        {
            return false;
        }

        MusicNodeParams = musicNodeParams;

        // fDuration (f64)
        Duration = reader.ReadDouble();

        // ulNumMarkers + pArrayMarkers
        var numMarkers = reader.ReadUInt32();

        for (var i = 0; i < numMarkers; i++)
        {
            var marker = new MusicMarker();

            if (!marker.Read(reader))
            {
                return false;
            }

            Markers.Add(marker);
        }

        return true;
    }
}

/// <summary>
///     Music marker (AkMusicMarkerWwise).
///     For v65-136 (v113 is in range).
/// </summary>
public class MusicMarker
{
    /// <summary>
    ///     Marker ID (entry=0, exit=1, custom cues use hash ID).
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    ///     Position in milliseconds.
    /// </summary>
    public double Position { get; set; }

    /// <summary>
    ///     Marker name (optional, for v65+).
    /// </summary>
    public string? Name { get; set; }

    public bool Read(BinaryReader reader)
    {
        // For v65+ (v113 is in range):
        // tid('id')
        Id = reader.ReadUInt32();

        // fPosition (f64)
        Position = reader.ReadDouble();

        // For v65-136: uStringSize + pMarkerName
        var stringSize = reader.ReadUInt32();

        if (stringSize > 0)
        {
            var nameBytes = reader.ReadBytes((int) stringSize);
            Name = Encoding.UTF8.GetString(nameBytes).TrimEnd('\0');
        }

        return true;
    }
}
