using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class ResumeActionSpecificParams
{
    public ResumeActionFlags Flags { get; set; }

    public bool IsMasterResume
    {
        get => Flags.HasFlag(ResumeActionFlags.IsMasterResume);
        set => Flags = value ? Flags | ResumeActionFlags.IsMasterResume : Flags & ~ResumeActionFlags.IsMasterResume;
    }

    public bool ApplyToStateTransitions
    {
        get => Flags.HasFlag(ResumeActionFlags.ApplyToStateTransitions);
        set =>
            Flags = value
                ? Flags | ResumeActionFlags.ApplyToStateTransitions
                : Flags & ~ResumeActionFlags.ApplyToStateTransitions;
    }

    public bool ApplyToDynamicSequence
    {
        get => Flags.HasFlag(ResumeActionFlags.ApplyToDynamicSequence);
        set =>
            Flags = value
                ? Flags | ResumeActionFlags.ApplyToDynamicSequence
                : Flags & ~ResumeActionFlags.ApplyToDynamicSequence;
    }

    public bool Read(BinaryReader reader)
    {
        Flags = (ResumeActionFlags) reader.ReadByte();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Flags);
    }
}
