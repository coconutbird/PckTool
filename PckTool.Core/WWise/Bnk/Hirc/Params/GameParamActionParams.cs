namespace PckTool.Core.WWise.Bnk.Hirc.Params;

/// <summary>
///     Game parameter action params for CAkActionSetGameParameter.
///     Corresponds to CAkActionSetValue::SetActionParams + CAkActionSetGameParameter::SetActionSpecificParams.
///     For v113.
/// </summary>
public class GameParamActionParams
{
    /// <summary>
    ///     Fade curve type. Uses 5 bits (0x1F mask) per wwiser.
    /// </summary>
    public byte BitVector { get; set; }

    public byte FadeCurve
    {
        get => (byte) (BitVector & 0x1F);
        set => BitVector = (byte) ((BitVector & 0xE0) | (value & 0x1F));
    }

    public GameParameterSpecificParams SpecificParams { get; set; } = null!;
    public ExceptParams ExceptParams { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // CAkActionSetValue::SetActionParams
        // For v>56: no TTime fields
        BitVector = reader.ReadByte();

        // CAkActionSetGameParameter::SetActionSpecificParams
        var specificParams = new GameParameterSpecificParams();

        if (!specificParams.Read(reader))
        {
            return false;
        }

        SpecificParams = specificParams;

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
        writer.Write(BitVector);
        SpecificParams.Write(writer);
        ExceptParams.Write(writer);
    }
}

/// <summary>
///     Game parameter specific params.
///     Corresponds to CAkActionSetGameParameter::SetActionSpecificParams in wwiser.
///     For v113 (v90+).
/// </summary>
public class GameParameterSpecificParams
{
    /// <summary>
    ///     Bypass transition flag. For v90+.
    /// </summary>
    public byte BypassTransition { get; set; }

    /// <summary>
    ///     Value meaning (AkValueMeaning). For v>56: u8.
    /// </summary>
    public byte ValueMeaning { get; set; }

    /// <summary>
    ///     Randomizer modifier base value.
    /// </summary>
    public float Base { get; set; }

    /// <summary>
    ///     Randomizer modifier min value.
    /// </summary>
    public float Min { get; set; }

    /// <summary>
    ///     Randomizer modifier max value.
    /// </summary>
    public float Max { get; set; }

    public bool Read(BinaryReader reader)
    {
        // For v90+: read bBypassTransition
        BypassTransition = reader.ReadByte();

        // For v>56: u8
        ValueMeaning = reader.ReadByte();

        // RANGED_PARAMETER<float>
        Base = reader.ReadSingle();
        Min = reader.ReadSingle();
        Max = reader.ReadSingle();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(BypassTransition);
        writer.Write(ValueMeaning);
        writer.Write(Base);
        writer.Write(Min);
        writer.Write(Max);
    }
}
