using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class NodeBaseParams
{
    public NodeInitialFxParams NodeInitialFxParams { get; set; }
    public byte OverrideAttachmentParams { get; set; }
    public uint OverrideBusId { get; set; }
    public uint DirectParentId { get; set; }
    public NodeBaseFlags Flags { get; set; }

    public bool PriorityOverrideParent
    {
        get => Flags.HasFlag(NodeBaseFlags.PriorityOverrideParent);
        set =>
            Flags = value
                ? Flags | NodeBaseFlags.PriorityOverrideParent
                : Flags & ~NodeBaseFlags.PriorityOverrideParent;
    }

    public bool PriorityApplyDistFactor
    {
        get => Flags.HasFlag(NodeBaseFlags.PriorityApplyDistFactor);
        set =>
            Flags = value
                ? Flags | NodeBaseFlags.PriorityApplyDistFactor
                : Flags & ~NodeBaseFlags.PriorityApplyDistFactor;
    }

    public bool OverrideMidiEventsBehavior
    {
        get => Flags.HasFlag(NodeBaseFlags.OverrideMidiEventsBehavior);
        set =>
            Flags = value
                ? Flags | NodeBaseFlags.OverrideMidiEventsBehavior
                : Flags & ~NodeBaseFlags.OverrideMidiEventsBehavior;
    }

    public bool OverrideMidiNoteTracking
    {
        get => Flags.HasFlag(NodeBaseFlags.OverrideMidiNoteTracking);
        set =>
            Flags = value
                ? Flags | NodeBaseFlags.OverrideMidiNoteTracking
                : Flags & ~NodeBaseFlags.OverrideMidiNoteTracking;
    }

    public bool EnableMidiNoteTracking
    {
        get => Flags.HasFlag(NodeBaseFlags.EnableMidiNoteTracking);
        set =>
            Flags = value
                ? Flags | NodeBaseFlags.EnableMidiNoteTracking
                : Flags & ~NodeBaseFlags.EnableMidiNoteTracking;
    }

    public bool IsMidiBreakLoopOnNoteOff
    {
        get => Flags.HasFlag(NodeBaseFlags.IsMidiBreakLoopOnNoteOff);
        set =>
            Flags = value
                ? Flags | NodeBaseFlags.IsMidiBreakLoopOnNoteOff
                : Flags & ~NodeBaseFlags.IsMidiBreakLoopOnNoteOff;
    }

    public NodeInitialParams NodeInitialParams { get; set; }
    public PositioningParams PositioningParams { get; set; }
    public AuxParams AuxParams { get; set; }
    public AdvSettingsParams AdvSettingsParams { get; set; }
    public StateChunk StateChunk { get; set; }
    public InitialRtpc InitialRtpc { get; set; }

    /// <summary>
    ///     FeedbackInfo.BusId - only present when the bank has feedback enabled (bFeedbackInBank != 0).
    ///     For v113 (≤126), this is read after InitialRtpc.
    /// </summary>
    public uint? FeedbackBusId { get; set; }

    public bool Read(BinaryReader reader, bool hasFeedback = false)
    {
        var nodeInitialFxParams = new NodeInitialFxParams();

        if (!nodeInitialFxParams.Read(reader))
        {
            return false;
        }

        var overrideAttachmentParams = reader.ReadByte();
        var overrideBusId = reader.ReadUInt32();
        var directParentId = reader.ReadUInt32();
        var bitVector = reader.ReadByte();

        var nodeInitialParams = new NodeInitialParams();

        if (!nodeInitialParams.Read(reader))
        {
            return false;
        }

        var positioningParams = new PositioningParams();

        if (!positioningParams.Read(reader))
        {
            return false;
        }

        var auxParams = new AuxParams();

        if (!auxParams.Read(reader))
        {
            return false;
        }

        var advSettingsParams = new AdvSettingsParams();

        if (!advSettingsParams.Read(reader))
        {
            return false;
        }

        var stateChunk = new StateChunk();

        if (!stateChunk.Read(reader))
        {
            return false;
        }

        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        // For v113 (≤126), read FeedbackInfo if the bank has feedback enabled
        // See CAkParameterNodeBase__ReadFeedbackInfo in wwiser wparser.py
        uint? feedbackBusId = null;

        if (hasFeedback)
        {
            feedbackBusId = reader.ReadUInt32();
        }

        NodeInitialFxParams = nodeInitialFxParams;

        OverrideAttachmentParams = overrideAttachmentParams;
        OverrideBusId = overrideBusId;
        DirectParentId = directParentId;
        Flags = (NodeBaseFlags) bitVector;

        NodeInitialParams = nodeInitialParams;
        PositioningParams = positioningParams;
        AuxParams = auxParams;
        AdvSettingsParams = advSettingsParams;
        StateChunk = stateChunk;
        InitialRtpc = initialRtpc;
        FeedbackBusId = feedbackBusId;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        NodeInitialFxParams.Write(writer);
        writer.Write(OverrideAttachmentParams);
        writer.Write(OverrideBusId);
        writer.Write(DirectParentId);
        writer.Write((byte) Flags);
        NodeInitialParams.Write(writer);
        PositioningParams.Write(writer);
        AuxParams.Write(writer);
        AdvSettingsParams.Write(writer);
        StateChunk.Write(writer);
        InitialRtpc.Write(writer);

        // Write FeedbackInfo.BusId if present
        if (FeedbackBusId.HasValue)
        {
            writer.Write(FeedbackBusId.Value);
        }
    }
}
