namespace PckTool.Core.WWise.Bnk.Enums;

/// <summary>
///     HIRC object types for bank version 113 (versions 73-126).
///     Note: Type IDs changed in version 127+.
/// </summary>
public enum HircType : byte
{
    State = 1,
    Sound = 2,
    Action = 3,
    Event = 4,
    RanSeqCntr = 5,
    SwitchCntr = 6,
    ActorMixer = 7,
    Bus = 8,
    LayerCntr = 9,
    Segment = 10,
    Track = 11,
    MusicSwitch = 12,
    MusicRanSeq = 13,
    Attenuation = 14,
    DialogueEvent = 15,
    FeedbackBus = 16,
    FeedbackNode = 17,
    FxShareSet = 18,
    FxCustom = 19,
    AuxBus = 20,
    LfoModulator = 21,      // v112>=
    EnvelopeModulator = 22, // v112>=
    AudioDevice = 23        // v118>=
}
