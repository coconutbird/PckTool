namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     BypassFX action params for CAkActionBypassFX.
///     Corresponds to CAkActionBypassFX::SetActionParams in wwiser.
///     For v113 (v26 &lt; v &lt;= v145).
/// </summary>
public class BypassFXActionParams
{
    /// <summary>
    ///     Whether to bypass FX (bIsBypass).
    /// </summary>
    public byte IsBypass { get; set; }

    /// <summary>
    ///     Target mask for which FX slots to bypass (uTargetMask).
    ///     For v26 &lt; v &lt;= v145.
    /// </summary>
    public byte TargetMask { get; set; }

    /// <summary>
    ///     Exception params.
    /// </summary>
    public ExceptParams ExceptParams { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // CAkActionBypassFX::SetActionParams
        IsBypass = reader.ReadByte();

        // For v26 < v <= v145: uTargetMask
        // For v > 145: byFxSlot (but we target v113)
        TargetMask = reader.ReadByte();

        // CAkActionExcept::SetExceptParams
        var exceptParams = new ExceptParams();

        if (!exceptParams.Read(reader))
        {
            return false;
        }

        ExceptParams = exceptParams;

        return true;
    }
}
