using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Helpers;

namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Value action params for CAkActionSetValue and its subclasses.
///     Corresponds to CAkActionSetValue::SetActionParams + specific params.
///     For v113.
/// </summary>
public class ValueActionParams
{
    public byte BitVector { get; set; }

    /// <summary>
    ///     Fade curve type. Uses 5 bits (0x1F mask) per wwiser.
    /// </summary>
    public byte FadeCurve
    {
        get => (byte) (BitVector & 0x1F);
        set => BitVector = (byte) ((BitVector & 0xE0) | (value & 0x1F));
    }

    /// <summary>
    ///     Prop action specific params. Only set for SetAkProp actions (SetVolume, SetPitch, etc.).
    ///     Null for Mute actions which use CAkAction::SetActionSpecificParams (empty).
    /// </summary>
    public PropActionSpecificParams? PropActionSpecificParams { get; set; }

    public ExceptParams ExceptParams { get; set; } = null!;

    public bool Read(BinaryReader reader, ActionType actionType)
    {
        // CAkActionSetValue::SetActionParams
        var bitVector = reader.ReadByte();

        // Dispatch to specific params based on action class
        // CAkActionMute uses empty specific params (CAkAction::SetActionSpecificParams)
        // CAkActionSetAkProp uses PropActionSpecificParams (CAkActionSetAkProp::SetActionSpecificParams)
        if (!ActionTypeHelpers.IsMuteActionType(actionType))
        {
            var propActionSpecificParams = new PropActionSpecificParams();

            if (!propActionSpecificParams.Read(reader))
            {
                return false;
            }

            PropActionSpecificParams = propActionSpecificParams;
        }

        // CAkActionExcept::SetExceptParams
        var exceptParams = new ExceptParams();

        if (!exceptParams.Read(reader))
        {
            return false;
        }

        BitVector = bitVector;
        ExceptParams = exceptParams;

        return true;
    }
}
