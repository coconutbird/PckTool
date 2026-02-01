namespace PckTool.Core.WWise.Bnk.Enums;

/// <summary>
///     Action category enum.
///     Represents different categories of actions based on their param structure.
///     Maps to wwiser's CAkAction class hierarchy.
/// </summary>
public enum ActionCategory : byte
{
    Unknown = 0,
    Play,      // CAkActionPlay - Play, PlayAndContinue (reads byBitVector + fileID)
    PlayEvent, // CAkActionPlayEvent - PlayEvent (empty params, v113+)
    Active,    // CAkActionActive - Stop, Pause, Resume
    Value,     // CAkActionSetValue - SetVolume, SetPitch, SetLPF, etc.
    State,     // CAkActionSetState - SetState
    Switch,    // CAkActionSetSwitch - SetSwitch
    GameParam, // CAkActionSetGameParameter - SetGameParameter
    BypassFX,  // CAkActionBypassFX - BypassFX, ResetBypassFX, SetBypassFXSlot, etc.
    Seek,      // CAkActionSeek - Seek actions
    Event,     // CAkActionEvent - StopEvent, PauseEvent, ResumeEvent, etc. (uses empty params)
    None       // CAkAction - UseState, Break, Trigger, Duck, etc. (uses empty params)
}
