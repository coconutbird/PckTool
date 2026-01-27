namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     State action params for CAkActionSetState.
///     Corresponds to CAkActionSetState::SetActionParams in wwiser.
/// </summary>
public class StateActionParams
{
    /// <summary>
    ///     State group ID (ulStateGroupID).
    /// </summary>
    public uint StateGroupId { get; set; }

    /// <summary>
    ///     Target state ID (ulTargetStateID).
    /// </summary>
    public uint TargetStateId { get; set; }

    public bool Read(BinaryReader reader)
    {
        StateGroupId = reader.ReadUInt32();
        TargetStateId = reader.ReadUInt32();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(StateGroupId);
        writer.Write(TargetStateId);
    }
}
