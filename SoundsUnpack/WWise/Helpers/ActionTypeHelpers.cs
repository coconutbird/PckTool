using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Helpers;

public static class ActionTypeHelpers
{
    public static ActionCategory GetActionCategory(ActionType actionType)
    {
        if (IsValueActionType(actionType))
        {
            return ActionCategory.Value;
        }

        if (IsActiveActionType(actionType))
        {
            return ActionCategory.Active;
        }

        if (IsPlayActionType(actionType))
        {
            return ActionCategory.Play;
        }

        throw new ArgumentOutOfRangeException(nameof(actionType), $"Unknown action category for type: {actionType}");
    }

    public static bool IsValidActionType(ActionType actionType)
    {
        var category = GetActionCategory(actionType);

        return category != ActionCategory.Unknown;
    }

    public static bool IsActiveActionType(ActionType actionType)
    {
        return IsStopActionType(actionType) || IsResumeActionType(actionType);
    }

    public static bool IsValueActionType(ActionType actionType)
    {
        if (IsMuteActionType(actionType))
        {
            return true;
        }

        switch (actionType)
        {
            // all set action types are value action types
            case ActionType.SetState:
            case ActionType.SetSwitch:
            case ActionType.SetVolume_M:
            case ActionType.SetVolume_O:
            case ActionType.SetPitch_M:
            case ActionType.SetPitch_O:
            case ActionType.SetLPF_M:
            case ActionType.SetLPF_O:
            case ActionType.SetHPF_M:
            case ActionType.SetHPF_O:
            case ActionType.SetBusVolume_M:
            case ActionType.SetBusVolume_O:
            case ActionType.SetGameParameter:
            case ActionType.SetGameParameter_O:
            case ActionType.SetFX_M:
            case ActionType.SetBypassFXSlot_M:
            case ActionType.SetBypassFXSlot_O:
            case ActionType.SetBypassAllFX_M:
                return true;

            case ActionType.ResetBypassFX_M:
            case ActionType.ResetBypassFX_O:
            case ActionType.ResetBypassFX_ALL:
            case ActionType.ResetBypassFX_ALL_O:
            case ActionType.ResetBypassFX_AE:
            case ActionType.ResetBypassFX_AE_O:
            case ActionType.ResetVolume_M:
            case ActionType.ResetVolume_O:
            case ActionType.ResetVolume_ALL:
            case ActionType.ResetVolume_ALL_O:
            case ActionType.ResetVolume_AE:
            case ActionType.ResetVolume_AE_O:
            case ActionType.ResetPitch_M:
            case ActionType.ResetPitch_O:
            case ActionType.ResetPitch_ALL:
            case ActionType.ResetPitch_ALL_O:
            case ActionType.ResetPitch_AE:
            case ActionType.ResetPitch_AE_O:
            case ActionType.ResetLPF_M:
            case ActionType.ResetLPF_O:
            case ActionType.ResetLPF_ALL:
            case ActionType.ResetLPF_ALL_O:
            case ActionType.ResetLPF_AE:
            case ActionType.ResetLPF_AE_O:
            case ActionType.ResetHPF_M:
            case ActionType.ResetHPF_O:
            case ActionType.ResetHPF_ALL:
            case ActionType.ResetHPF_ALL_O:
            case ActionType.ResetHPF_AE:
            case ActionType.ResetHPF_AE_O:
            case ActionType.ResetBusVolume_M:
            case ActionType.ResetBusVolume_O:
            case ActionType.ResetBusVolume_ALL:
            case ActionType.ResetBusVolume_AE:
            case ActionType.ResetGameParameter:
            case ActionType.ResetGameParameter_O:
            case ActionType.ResetPlaylist_E:
            case ActionType.ResetPlaylist_E_O:
            case ActionType.ResetSetFX_M:
            case ActionType.ResetSetFX_ALL:
            case ActionType.ResetBypassFXSlot_M:
            case ActionType.ResetBypassFXSlot_O:
            case ActionType.ResetBypassFXSlot_ALL:
            case ActionType.ResetBypassFXSlot_ALL_O:
            case ActionType.ResetBypassAllFX_M:
            case ActionType.ResetBypassAllFX_O:
            case ActionType.ResetBypassAllFX_ALL:
            case ActionType.ResetBypassAllFX_ALL_O:
            case ActionType.ResetAllBypassFX_M:
            case ActionType.ResetAllBypassFX_O:
            case ActionType.ResetAllBypassFX_ALL:
            case ActionType.ResetAllBypassFX_ALL_O:
                return true;

            default:
                return false;
        }
    }

    public static bool IsStopActionType(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Stop_E:
            case ActionType.Stop_E_O:
            case ActionType.Stop_ALL:
            case ActionType.Stop_ALL_O:
            case ActionType.Stop_AE:
            case ActionType.Stop_AE_O:
            case ActionType.StopEvent:
                return true;

            default:
                return false;
        }
    }

    public static bool IsResumeActionType(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Resume_E:
            case ActionType.Resume_E_O:
            case ActionType.Resume_ALL:
            case ActionType.Resume_ALL_O:
            case ActionType.Resume_AE:
            case ActionType.Resume_AE_O:
            case ActionType.ResumeEvent:
                return true;

            default:
                return false;
        }
    }

    public static bool IsMuteActionType(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Mute_M:
            case ActionType.Mute_O:
                return true;

            default:
                return false;
        }
    }

    private static bool IsPlayActionType(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Play:
            case ActionType.PlayAndContinue:
            case ActionType.PlayEvent:
            case ActionType.PlayEventUnknown_O:
                return true;

            default:
                return false;
        }
    }
}
