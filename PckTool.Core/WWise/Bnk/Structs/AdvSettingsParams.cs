using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

public class AdvSettingsParams
{
    public AdvSettingsFlags Flags { get; set; }

    public bool KillNewest
    {
        get => Flags.HasFlag(AdvSettingsFlags.KillNewest);
        set => Flags = value ? Flags | AdvSettingsFlags.KillNewest : Flags & ~AdvSettingsFlags.KillNewest;
    }

    public bool UseVirtualBehavior
    {
        get => Flags.HasFlag(AdvSettingsFlags.UseVirtualBehavior);
        set =>
            Flags = value
                ? Flags | AdvSettingsFlags.UseVirtualBehavior
                : Flags & ~AdvSettingsFlags.UseVirtualBehavior;
    }

    public bool IgnoreParentMaxNumInst
    {
        get => Flags.HasFlag(AdvSettingsFlags.IgnoreParentMaxNumInst);
        set =>
            Flags = value
                ? Flags | AdvSettingsFlags.IgnoreParentMaxNumInst
                : Flags & ~AdvSettingsFlags.IgnoreParentMaxNumInst;
    }

    public bool IsVVoicesOptOverrideParent
    {
        get => Flags.HasFlag(AdvSettingsFlags.IsVVoicesOptOverrideParent);
        set =>
            Flags = value
                ? Flags | AdvSettingsFlags.IsVVoicesOptOverrideParent
                : Flags & ~AdvSettingsFlags.IsVVoicesOptOverrideParent;
    }

    public byte VirtualQueueBehavior { get; set; }
    public ushort MaxNumberOfInstances { get; set; }
    public ushort BelowThresholdBehavior { get; set; }

    public AdvSettingsFlags2 Flags2 { get; set; }

    public bool OverrideHdrEnvelope
    {
        get => Flags2.HasFlag(AdvSettingsFlags2.OverrideHdrEnvelope);
        set =>
            Flags2 = value
                ? Flags2 | AdvSettingsFlags2.OverrideHdrEnvelope
                : Flags2 & ~AdvSettingsFlags2.OverrideHdrEnvelope;
    }

    public bool OverrideAnalysis
    {
        get => Flags2.HasFlag(AdvSettingsFlags2.OverrideAnalysis);
        set =>
            Flags2 = value
                ? Flags2 | AdvSettingsFlags2.OverrideAnalysis
                : Flags2 & ~AdvSettingsFlags2.OverrideAnalysis;
    }

    public bool NormalizeLoudness
    {
        get => Flags2.HasFlag(AdvSettingsFlags2.NormalizeLoudness);
        set =>
            Flags2 = value
                ? Flags2 | AdvSettingsFlags2.NormalizeLoudness
                : Flags2 & ~AdvSettingsFlags2.NormalizeLoudness;
    }

    public bool EnableEnvelope
    {
        get => Flags2.HasFlag(AdvSettingsFlags2.EnableEnvelope);
        set =>
            Flags2 = value
                ? Flags2 | AdvSettingsFlags2.EnableEnvelope
                : Flags2 & ~AdvSettingsFlags2.EnableEnvelope;
    }

    public bool Read(BinaryReader reader)
    {
        Flags = (AdvSettingsFlags) reader.ReadByte();
        VirtualQueueBehavior = reader.ReadByte();
        MaxNumberOfInstances = reader.ReadUInt16();
        BelowThresholdBehavior = reader.ReadByte();
        Flags2 = (AdvSettingsFlags2) reader.ReadByte();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Flags);
        writer.Write(VirtualQueueBehavior);
        writer.Write(MaxNumberOfInstances);
        writer.Write((byte) BelowThresholdBehavior);
        writer.Write((byte) Flags2);
    }
}
