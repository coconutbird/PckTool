namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     Clip automation item (AkClipAutomation) for MusicTrack.
///     For v65+.
/// </summary>
public class ClipAutomation
{
    /// <summary>
    ///     Clip index.
    /// </summary>
    public uint ClipIndex { get; set; }

    /// <summary>
    ///     Automation type (AkClipAutomationType).
    /// </summary>
    public uint AutoType { get; set; }

    /// <summary>
    ///     Graph points for the automation curve (AkRTPCGraphPoint).
    /// </summary>
    public List<RtpcGraphPointBase<float>> GraphPoints { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        ClipIndex = reader.ReadUInt32();
        AutoType = reader.ReadUInt32();

        var numPoints = reader.ReadUInt32();

        for (var i = 0; i < numPoints; i++)
        {
            var point = new RtpcGraphPointBase<float>();

            if (!point.Read(reader))
            {
                return false;
            }

            GraphPoints.Add(point);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ClipIndex);
        writer.Write(AutoType);

        writer.Write((uint) GraphPoints.Count);

        foreach (var point in GraphPoints)
        {
            point.Write(writer);
        }
    }
}
