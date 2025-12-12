namespace PckTool.WWise.Structs;

/// <summary>
///     Switch action params for CAkActionSetSwitch.
///     Corresponds to CAkActionSetSwitch::SetActionParams in wwiser.
/// </summary>
public class SwitchActionParams
{
    /// <summary>
    ///     Switch group ID (ulSwitchGroupID).
    /// </summary>
    public uint SwitchGroupId { get; set; }

    /// <summary>
    ///     Switch state ID (ulSwitchStateID).
    /// </summary>
    public uint SwitchStateId { get; set; }

    public bool Read(BinaryReader reader)
    {
        SwitchGroupId = reader.ReadUInt32();
        SwitchStateId = reader.ReadUInt32();

        return true;
    }
}
