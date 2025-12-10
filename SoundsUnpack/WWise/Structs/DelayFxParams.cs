namespace SoundsUnpack.WWise.Structs;

public class DelayFxParams
{
    public float NonRtpcDelayTime { get; set; }
    public float RtpcFeedback { get; set; }
    public float RtpcWetDryMix { get; set; }
    public float RtpcOutputLevel { get; set; }
    public byte RtpcFeedbackEnabled { get; set; }
    public byte NonRtpcProcessLFE { get; set; }

    public bool Read(BinaryReader reader)
    {
        var nonRtpcDelayTime = reader.ReadSingle();
        var rtpcFeedback = reader.ReadSingle();
        var rtpcWetDryMix = reader.ReadSingle();
        var rtpcOutputLevel = reader.ReadSingle();
        var rtpcFeedbackEnabled = reader.ReadByte();
        var nonRtpcProcessLFE = reader.ReadByte();

        NonRtpcDelayTime = nonRtpcDelayTime;
        RtpcFeedback = rtpcFeedback;
        RtpcWetDryMix = rtpcWetDryMix;
        RtpcOutputLevel = rtpcOutputLevel;
        RtpcFeedbackEnabled = rtpcFeedbackEnabled;
        NonRtpcProcessLFE = nonRtpcProcessLFE;

        return true;
    }
}
