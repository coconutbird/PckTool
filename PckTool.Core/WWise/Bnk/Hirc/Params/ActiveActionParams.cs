using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

/// <summary>
///     Active action params for bank version 113.
///     Corresponds to CAkActionActive::SetActionParams in wwiser.
///     Used by Stop, Pause, and Resume action types.
/// </summary>
public class ActiveActionParams
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

    public StopActionSpecificParams? StopActionSpecificParams { get; set; }
    public PauseActionSpecificParams? PauseActionSpecificParams { get; set; }
    public ResumeActionSpecificParams? ResumeActionSpecificParams { get; set; }
    public ExceptParams ExceptParams { get; set; } = null!;

    public bool Read(BinaryReader reader, ActionType actionType)
    {
        // CAkActionActive::SetActionParams
        // For v>56: no TTime fields, just byBitVector
        var bitVector = reader.ReadByte();

        // CAkClass__SetActionSpecificParams - depends on action type
        // For v113: Stop has specific params (v125+), Pause and Resume have specific params
        if (ActionTypeHelpers.IsStopActionType(actionType))
        {
            // For v125+, Stop has specific params
            // For v<=122, Stop uses CAkAction__SetActionSpecificParams (empty)
            // v113 falls in <=122 range, so no specific params for Stop
        }
        else if (ActionTypeHelpers.IsPauseActionType(actionType))
        {
            var pauseParams = new PauseActionSpecificParams();

            if (!pauseParams.Read(reader))
            {
                return false;
            }

            PauseActionSpecificParams = pauseParams;
        }
        else if (ActionTypeHelpers.IsResumeActionType(actionType))
        {
            var resumeParams = new ResumeActionSpecificParams();

            if (!resumeParams.Read(reader))
            {
                return false;
            }

            ResumeActionSpecificParams = resumeParams;
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

    public void Write(BinaryWriter writer, ActionType actionType)
    {
        writer.Write(BitVector);

        if (ActionTypeHelpers.IsStopActionType(actionType))
        {
            // v113 doesn't have specific params for Stop
        }
        else if (ActionTypeHelpers.IsPauseActionType(actionType))
        {
            PauseActionSpecificParams?.Write(writer);
        }
        else if (ActionTypeHelpers.IsResumeActionType(actionType))
        {
            ResumeActionSpecificParams?.Write(writer);
        }

        ExceptParams.Write(writer);
    }
}

/// <summary>
///     Stop action specific params.
///     Corresponds to CAkActionStop::SetActionSpecificParams in wwiser.
///     Only used for v125+; for v113, Stop uses empty CAkAction__SetActionSpecificParams.
/// </summary>
public class StopActionSpecificParams
{
    public StopActionFlags Flags { get; set; }

    public bool ApplyToStateTransitions
    {
        get => Flags.HasFlag(StopActionFlags.ApplyToStateTransitions);
        set =>
            Flags = value
                ? Flags | StopActionFlags.ApplyToStateTransitions
                : Flags & ~StopActionFlags.ApplyToStateTransitions;
    }

    public bool ApplyToDynamicSequence
    {
        get => Flags.HasFlag(StopActionFlags.ApplyToDynamicSequence);
        set =>
            Flags = value
                ? Flags | StopActionFlags.ApplyToDynamicSequence
                : Flags & ~StopActionFlags.ApplyToDynamicSequence;
    }

    public bool Read(BinaryReader reader)
    {
        Flags = (StopActionFlags) reader.ReadByte();

        return true;
    }
}

/// <summary>
///     Pause action specific params.
///     Corresponds to CAkActionPause::SetActionSpecificParams in wwiser.
///     For v63+: u8 with bIncludePendingResume, bApplyToStateTransitions, bApplyToDynamicSequence bits.
/// </summary>
public class PauseActionSpecificParams
{
    public PauseActionFlags Flags { get; set; }

    public bool IncludePendingResume
    {
        get => Flags.HasFlag(PauseActionFlags.IncludePendingResume);
        set =>
            Flags = value
                ? Flags | PauseActionFlags.IncludePendingResume
                : Flags & ~PauseActionFlags.IncludePendingResume;
    }

    public bool ApplyToStateTransitions
    {
        get => Flags.HasFlag(PauseActionFlags.ApplyToStateTransitions);
        set =>
            Flags = value
                ? Flags | PauseActionFlags.ApplyToStateTransitions
                : Flags & ~PauseActionFlags.ApplyToStateTransitions;
    }

    public bool ApplyToDynamicSequence
    {
        get => Flags.HasFlag(PauseActionFlags.ApplyToDynamicSequence);
        set =>
            Flags = value
                ? Flags | PauseActionFlags.ApplyToDynamicSequence
                : Flags & ~PauseActionFlags.ApplyToDynamicSequence;
    }

    public bool Read(BinaryReader reader)
    {
        Flags = (PauseActionFlags) reader.ReadByte();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Flags);
    }
}
