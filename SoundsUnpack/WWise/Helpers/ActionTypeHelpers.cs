using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Helpers;

/// <summary>
///     Helper methods for determining action type categories.
///     Based on wwiser's wparser_cls.py CAkAction dispatch table.
/// </summary>
public static class ActionTypeHelpers
{
    /// <summary>
    ///     Gets the action category for the given action type.
    ///     Maps to wwiser's CAkAction class hierarchy for v113.
    /// </summary>
    public static ActionCategory GetActionCategory(ActionType actionType)
    {
        if (IsPlayActionType(actionType)) return ActionCategory.Play;

        if (IsActiveActionType(actionType)) return ActionCategory.Active;

        if (IsStateActionType(actionType)) return ActionCategory.State;

        if (IsSwitchActionType(actionType)) return ActionCategory.Switch;

        if (IsGameParamActionType(actionType)) return ActionCategory.GameParam;

        if (IsValueActionType(actionType)) return ActionCategory.Value;

        if (IsBypassFXActionType(actionType)) return ActionCategory.BypassFX;

        if (IsNoneParamsActionType(actionType)) return ActionCategory.None;

        return ActionCategory.Unknown;
    }

    // ============================================
    // Play Actions - CAkActionPlay
    // ============================================

    public static bool IsPlayActionType(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.Play => true,
            ActionType.PlayAndContinue => true,
            ActionType.PlayEvent => true,
            ActionType.PlayEventUnknown_O => true,
            _ => false
        };
    }

    // ============================================
    // Active Actions - CAkActionActive (Stop, Pause, Resume)
    // ============================================

    public static bool IsActiveActionType(ActionType actionType)
    {
        return IsStopActionType(actionType) || IsPauseActionType(actionType) || IsResumeActionType(actionType);
    }

    public static bool IsStopActionType(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.Stop_E => true,
            ActionType.Stop_E_O => true,
            ActionType.Stop_ALL => true,
            ActionType.Stop_ALL_O => true,
            ActionType.Stop_AE => true,
            ActionType.Stop_AE_O => true,
            ActionType.StopEvent => true,
            _ => false
        };
    }

    public static bool IsPauseActionType(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.Pause_E => true,
            ActionType.Pause_E_O => true,
            ActionType.Pause_ALL => true,
            ActionType.Pause_ALL_O => true,
            ActionType.Pause_AE => true,
            ActionType.Pause_AE_O => true,
            ActionType.PauseEvent => true,
            _ => false
        };
    }

    public static bool IsResumeActionType(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.Resume_E => true,
            ActionType.Resume_E_O => true,
            ActionType.Resume_ALL => true,
            ActionType.Resume_ALL_O => true,
            ActionType.Resume_AE => true,
            ActionType.Resume_AE_O => true,
            ActionType.ResumeEvent => true,
            _ => false
        };
    }

    // ============================================
    // State Actions - CAkActionSetState
    // Has unique params: StateGroupID, TargetStateID
    // ============================================

    public static bool IsStateActionType(ActionType actionType)
    {
        return actionType == ActionType.SetState;
    }

    // ============================================
    // Switch Actions - CAkActionSetSwitch
    // Has unique params: SwitchGroupID, SwitchStateID
    // ============================================

    public static bool IsSwitchActionType(ActionType actionType)
    {
        return actionType == ActionType.SetSwitch;
    }

    // ============================================
    // GameParameter Actions - CAkActionSetGameParameter
    // Uses SetValue params + GameParameter specific params
    // ============================================

    public static bool IsGameParamActionType(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.SetGameParameter => true,
            ActionType.SetGameParameter_O => true,
            ActionType.ResetGameParameter => true,
            ActionType.ResetGameParameter_O => true,
            _ => false
        };
    }

    // ============================================
    // Value Actions - CAkActionSetValue (Mute, SetVolume, SetPitch, SetLPF, etc.)
    // Uses SetValue params with PropActionSpecificParams
    // ============================================

    public static bool IsValueActionType(ActionType actionType)
    {
        // Mute/Unmute
        if (IsMuteActionType(actionType)) return true;

        return actionType switch
        {
            // SetVolume
            ActionType.SetVolume_M => true,
            ActionType.SetVolume_O => true,
            ActionType.ResetVolume_M => true,
            ActionType.ResetVolume_O => true,
            ActionType.ResetVolume_ALL => true,
            ActionType.ResetVolume_ALL_O => true,
            ActionType.ResetVolume_AE => true,
            ActionType.ResetVolume_AE_O => true,

            // SetPitch
            ActionType.SetPitch_M => true,
            ActionType.SetPitch_O => true,
            ActionType.ResetPitch_M => true,
            ActionType.ResetPitch_O => true,
            ActionType.ResetPitch_ALL => true,
            ActionType.ResetPitch_ALL_O => true,
            ActionType.ResetPitch_AE => true,
            ActionType.ResetPitch_AE_O => true,

            // SetLPF
            ActionType.SetLPF_M => true,
            ActionType.SetLPF_O => true,
            ActionType.ResetLPF_M => true,
            ActionType.ResetLPF_O => true,
            ActionType.ResetLPF_ALL => true,
            ActionType.ResetLPF_ALL_O => true,
            ActionType.ResetLPF_AE => true,
            ActionType.ResetLPF_AE_O => true,

            // SetHPF
            ActionType.SetHPF_M => true,
            ActionType.SetHPF_O => true,
            ActionType.ResetHPF_M => true,
            ActionType.ResetHPF_O => true,
            ActionType.ResetHPF_ALL => true,
            ActionType.ResetHPF_ALL_O => true,
            ActionType.ResetHPF_AE => true,
            ActionType.ResetHPF_AE_O => true,

            // SetBusVolume (AkPropID_BusVolume)
            ActionType.SetBusVolume_M => true,
            ActionType.SetBusVolume_O => true,
            ActionType.ResetBusVolume_M => true,
            ActionType.ResetBusVolume_O => true,
            ActionType.ResetBusVolume_ALL => true,
            ActionType.ResetBusVolume_AE => true,

            _ => false
        };
    }

    public static bool IsMuteActionType(ActionType actionType)
    {
        return actionType switch
        {
            ActionType.Mute_M => true,
            ActionType.Mute_O => true,
            ActionType.Unmute_M => true,
            ActionType.Unmute_O => true,
            ActionType.Unmute_ALL => true,
            ActionType.Unmute_ALL_O => true,
            ActionType.Unmute_AE => true,
            ActionType.Unmute_AE_O => true,
            _ => false
        };
    }

    // ============================================
    // BypassFX Actions - CAkActionBypassFX
    // Has unique params: IsBypass, TargetMask, ExceptParams
    // ============================================

    public static bool IsBypassFXActionType(ActionType actionType)
    {
        return actionType switch
        {
            // BypassFX (0x1Axx, 0x1Bxx)
            ActionType.BypassFX_M => true,
            ActionType.BypassFX_O => true,
            ActionType.ResetBypassFX_M => true,
            ActionType.ResetBypassFX_O => true,
            ActionType.ResetBypassFX_ALL => true,
            ActionType.ResetBypassFX_ALL_O => true,
            ActionType.ResetBypassFX_AE => true,
            ActionType.ResetBypassFX_AE_O => true,

            // BypassFXSlot (0x33xx, 0x34xx)
            ActionType.SetBypassFXSlot_M => true,
            ActionType.SetBypassFXSlot_O => true,
            ActionType.ResetBypassFXSlot_M => true,
            ActionType.ResetBypassFXSlot_O => true,
            ActionType.ResetBypassFXSlot_ALL => true,
            ActionType.ResetBypassFXSlot_ALL_O => true,

            // BypassAllFX (0x35xx, 0x36xx, 0x37xx)
            ActionType.SetBypassAllFX_M => true,
            ActionType.SetBypassAllFX_O => true,
            ActionType.ResetBypassAllFX_M => true,
            ActionType.ResetBypassAllFX_O => true,
            ActionType.ResetBypassAllFX_ALL => true,
            ActionType.ResetBypassAllFX_ALL_O => true,
            ActionType.ResetAllBypassFX_M => true,
            ActionType.ResetAllBypassFX_O => true,
            ActionType.ResetAllBypassFX_ALL => true,
            ActionType.ResetAllBypassFX_ALL_O => true,

            _ => false
        };
    }

    // ============================================
    // None Params Actions - CAkAction (empty SetActionParams)
    // UseState, Break, Trigger, Duck, Release, Seek, SetFX, etc.
    // ============================================

    public static bool IsNoneParamsActionType(ActionType actionType)
    {
        return actionType switch
        {
            // UseState/UnuseState
            ActionType.UseState_E => true,
            ActionType.UnuseState_E => true,

            // Break
            ActionType.Break_E => true,
            ActionType.Break_E_O => true,

            // Trigger
            ActionType.Trigger => true,
            ActionType.Trigger_O => true,
            ActionType.Trigger_E => true,
            ActionType.Trigger_E_O => true,
            ActionType.Trigger_150 => true,
            ActionType.Trigger_O_150 => true,

            // Duck
            ActionType.Duck => true,

            // Release (has its own params but treating as None for now)
            ActionType.Release => true,
            ActionType.Release_O => true,

            // Seek (has its own params but treating as None for now)
            ActionType.Seek_E => true,
            ActionType.Seek_E_O => true,
            ActionType.Seek_ALL => true,
            ActionType.Seek_ALL_O => true,
            ActionType.Seek_AE => true,
            ActionType.Seek_AE_O => true,

            // ResetPlaylist
            ActionType.ResetPlaylist_E => true,
            ActionType.ResetPlaylist_E_O => true,

            // SetFX (has its own params but treating as None for now)
            ActionType.SetFX_M => true,
            ActionType.ResetSetFX_M => true,
            ActionType.ResetSetFX_ALL => true,

            // NoOp
            ActionType.NoOp => true,
            ActionType.None => true,

            _ => false
        };
    }
}
