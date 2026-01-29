namespace PckTool.Core.WWise.Bnk.Enums;

/// <summary>
///     Flags for ResumeActionSpecificParams.
/// </summary>
[Flags]
public enum ResumeActionFlags : byte
{
    None = 0,
    IsMasterResume = 1 << 0,          // 0x01
    ApplyToStateTransitions = 1 << 1, // 0x02
    ApplyToDynamicSequence = 1 << 2   // 0x04
}

/// <summary>
///     Flags for PauseActionSpecificParams.
/// </summary>
[Flags]
public enum PauseActionFlags : byte
{
    None = 0,
    IncludePendingResume = 1 << 0,    // 0x01
    ApplyToStateTransitions = 1 << 1, // 0x02
    ApplyToDynamicSequence = 1 << 2   // 0x04
}

/// <summary>
///     Flags for StopActionSpecificParams.
/// </summary>
[Flags]
public enum StopActionFlags : byte
{
    None = 0,
    ApplyToStateTransitions = 1 << 1, // 0x02
    ApplyToDynamicSequence = 1 << 2   // 0x04
}

/// <summary>
///     Flags for AdvSettingsParams BitVector.
/// </summary>
[Flags]
public enum AdvSettingsFlags : byte
{
    None = 0,
    KillNewest = 1 << 0,                // 0x01
    UseVirtualBehavior = 1 << 1,        // 0x02
    IgnoreParentMaxNumInst = 1 << 2,    // 0x04
    IsVVoicesOptOverrideParent = 1 << 3 // 0x08
}

/// <summary>
///     Flags for AdvSettingsParams BitVector2.
/// </summary>
[Flags]
public enum AdvSettingsFlags2 : byte
{
    None = 0,
    OverrideHdrEnvelope = 1 << 0, // 0x01
    OverrideAnalysis = 1 << 1,    // 0x02
    NormalizeLoudness = 1 << 2,   // 0x04
    EnableEnvelope = 1 << 3       // 0x08
}

/// <summary>
///     Flags for AuxParams BitVector.
///     Note: OverrideUserAuxSends is bit 2 (0x04), HasAux is bit 3 (0x08), OverrideReflectionsAuxBus is bit 4 (0x10).
/// </summary>
[Flags]
public enum AuxFlags : byte
{
    None = 0,
    OverrideUserAuxSends = 1 << 2,     // 0x04
    HasAux = 1 << 3,                   // 0x08
    OverrideReflectionsAuxBus = 1 << 4 // 0x10
}

/// <summary>
///     Flags for NodeBaseParams ByBitVector.
/// </summary>
[Flags]
public enum NodeBaseFlags : byte
{
    None = 0,
    PriorityOverrideParent = 1 << 0,     // 0x01
    PriorityApplyDistFactor = 1 << 1,    // 0x02
    OverrideMidiEventsBehavior = 1 << 2, // 0x04
    OverrideMidiNoteTracking = 1 << 3,   // 0x08
    EnableMidiNoteTracking = 1 << 4,     // 0x10
    IsMidiBreakLoopOnNoteOff = 1 << 5    // 0x20
}

/// <summary>
///     Flags for PositioningParams BitVector.
/// </summary>
[Flags]
public enum PositioningFlags : byte
{
    None = 0,
    PositioningInfoOverrideParent = 1 << 0, // 0x01
    Enable2D = 1 << 1,                      // 0x02
    EnableSpatialization = 1 << 2,          // 0x04
    Is3DPositioningAvailable = 1 << 3       // 0x08
}

/// <summary>
///     Flags for 3D positioning (Bits3D in PositioningParams).
///     Bits 0-1: e3DPositionType (not a flag, extracted as 0x03 mask).
/// </summary>
[Flags]
public enum Positioning3DFlags : byte
{
    None = 0,

    // Bit 3: bHoldEmitterPosAndOrient
    HoldEmitterPosAndOrient = 1 << 3, // 0x08

    // Bit 4: bHoldListenerOrient
    HoldListenerOrient = 1 << 4 // 0x10
}

/// <summary>
///     Flags for BusInitialParams BitVector1.
/// </summary>
[Flags]
public enum BusFlags1 : byte
{
    None = 0,
    MainOutputHierarchy = 1 << 0, // 0x01
    IsBackgroundMusic = 1 << 1    // 0x02
}

/// <summary>
///     Flags for BusInitialParams BitVector2.
/// </summary>
[Flags]
public enum BusFlags2 : byte
{
    None = 0,
    KillNewest = 1 << 0,        // 0x01
    UseVirtualBehavior = 1 << 1 // 0x02
}

/// <summary>
///     Flags for BusInitialParams BitVector3.
/// </summary>
[Flags]
public enum BusFlags3 : byte
{
    None = 0,
    IsHdrBus = 1 << 0,                 // 0x01
    HdrReleaseModeExponential = 1 << 1 // 0x02
}
