namespace PckTool.Core.WWise.Bnk.Hirc.Params;

/// <summary>
///     Seek action params.
///     Corresponds to CAkActionSeek::SetActionParams in wwiser for v48+.
/// </summary>
public class SeekActionParams
{
    /// <summary>
    ///     Whether the seek value is relative to the duration (percentage).
    /// </summary>
    public byte IsSeekRelativeToDuration { get; set; }

    /// <summary>
    ///     The seek position value and randomizer range.
    /// </summary>
    public RandomizerModifier SeekValue { get; set; } = null!;

    /// <summary>
    ///     Whether to snap to the nearest marker.
    /// </summary>
    public byte SnapToNearestMarker { get; set; }

    /// <summary>
    ///     Exception parameters.
    /// </summary>
    public ExceptParams ExceptParams { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // bIsSeekRelativeToDuration (U8)
        IsSeekRelativeToDuration = reader.ReadByte();

        // RandomizerModifier: fSeekValue, fSeekValueMin, fSeekValueMax
        var seekValue = new RandomizerModifier();

        if (!seekValue.Read(reader))
        {
            return false;
        }

        SeekValue = seekValue;

        // bSnapToNearestMarker (U8)
        SnapToNearestMarker = reader.ReadByte();

        // CAkActionExcept::SetExceptParams
        var exceptParams = new ExceptParams();

        if (!exceptParams.Read(reader))
        {
            return false;
        }

        ExceptParams = exceptParams;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(IsSeekRelativeToDuration);
        SeekValue.Write(writer);
        writer.Write(SnapToNearestMarker);
        ExceptParams.Write(writer);
    }
}
