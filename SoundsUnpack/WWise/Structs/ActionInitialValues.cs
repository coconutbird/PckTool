using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Helpers;

namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Action initial values.
///     Corresponds to CAkAction::SetInitialValues in wwiser for v113.
/// </summary>
public class ActionInitialValues
{
    public uint Ext { get; set; }
    public byte Ext4 { get; set; }

    public bool IsBus
    {
        get => (Ext4 & 0x01) != 0;
        set => Ext4 = value ? (byte) (Ext4 | 0x01) : (byte) (Ext4 & 0xFE);
    }

    public PropBundle PropBundle1 { get; set; } = null!;
    public PropBundle PropBundle2 { get; set; } = null!;

    // Action params - only one will be set based on action category
    public PlayActionParams? PlayActionParams { get; set; }
    public ActiveActionParams? ActiveActionParams { get; set; }
    public StateActionParams? StateActionParams { get; set; }
    public SwitchActionParams? SwitchActionParams { get; set; }
    public GameParamActionParams? GameParamActionParams { get; set; }
    public ValueActionParams? ValueActionParams { get; set; }
    public BypassFXActionParams? BypassFXActionParams { get; set; }

    public bool Read(BinaryReader reader, ActionType actionType)
    {
        // CAkAction::SetInitialValues
        // For v>65: idExt (u32), idExt_4 (u8)
        var ext = reader.ReadUInt32();
        var ext4 = reader.ReadByte();

        // PropBundle1 - AkPropBundle<AkPropValue, unsigned char>::SetInitialParams
        var propBundle1 = new PropBundle();

        if (!propBundle1.Read(reader))
        {
            return false;
        }

        // PropBundle2 - AkPropBundle<RANGED_MODIFIERS<AkPropValue>, unsigned char>::SetInitialParams
        var propBundle2 = new PropBundle();

        if (!propBundle2.Read(reader, true))
        {
            return false;
        }

        // CAkClass::SetActionParams - dispatches to specific action params
        var actionCategory = ActionTypeHelpers.GetActionCategory(actionType);

        switch (actionCategory)
        {
            case ActionCategory.Play:
            {
                var playParams = new PlayActionParams();

                if (!playParams.Read(reader))
                {
                    return false;
                }

                PlayActionParams = playParams;

                break;
            }

            case ActionCategory.Active:
            {
                var activeParams = new ActiveActionParams();

                if (!activeParams.Read(reader, actionType))
                {
                    return false;
                }

                ActiveActionParams = activeParams;

                break;
            }

            case ActionCategory.State:
            {
                var stateParams = new StateActionParams();

                if (!stateParams.Read(reader))
                {
                    return false;
                }

                StateActionParams = stateParams;

                break;
            }

            case ActionCategory.Switch:
            {
                var switchParams = new SwitchActionParams();

                if (!switchParams.Read(reader))
                {
                    return false;
                }

                SwitchActionParams = switchParams;

                break;
            }

            case ActionCategory.GameParam:
            {
                var gameParamParams = new GameParamActionParams();

                if (!gameParamParams.Read(reader))
                {
                    return false;
                }

                GameParamActionParams = gameParamParams;

                break;
            }

            case ActionCategory.Value:
            {
                var valueParams = new ValueActionParams();

                if (!valueParams.Read(reader, actionType))
                {
                    return false;
                }

                ValueActionParams = valueParams;

                break;
            }

            case ActionCategory.BypassFX:
            {
                var bypassFXParams = new BypassFXActionParams();

                if (!bypassFXParams.Read(reader))
                {
                    return false;
                }

                BypassFXActionParams = bypassFXParams;

                break;
            }

            case ActionCategory.None:
            case ActionCategory.Event:
                // CAkAction::SetActionParams - empty, no additional params
                break;

            case ActionCategory.Unknown:
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(actionCategory),
                    $"Unsupported action category: {actionCategory} for action type: {actionType}");
        }

        Ext = ext;
        Ext4 = ext4;
        PropBundle1 = propBundle1;
        PropBundle2 = propBundle2;

        return true;
    }
}
