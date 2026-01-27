namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     Modulator initial values for bank version 113.
///     Corresponds to CAkModulator::SetInitialValues in wwiser.
///     Used by both LFOModulator and EnvelopeModulator.
/// </summary>
public class ModulatorInitialValues
{
    /// <summary>
    ///     Property bundle (values).
    /// </summary>
    public PropBundle PropBundle { get; set; } = null!;

    /// <summary>
    ///     Property bundle (ranged modifiers).
    /// </summary>
    public PropBundle PropBundleRanged { get; set; } = null!;

    /// <summary>
    ///     Initial RTPC data.
    /// </summary>
    public InitialRtpc InitialRtpc { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // AkPropBundle<AkPropValue> (values)
        var propBundle = new PropBundle();

        if (!propBundle.Read(reader))
        {
            return false;
        }

        PropBundle = propBundle;

        // AkPropBundle<RANGED_MODIFIERS<AkPropValue>> (ranged modifiers)
        var propBundleRanged = new PropBundle();

        if (!propBundleRanged.Read(reader, true))
        {
            return false;
        }

        PropBundleRanged = propBundleRanged;

        // SetInitialRTPC<CAkModulator>
        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        InitialRtpc = initialRtpc;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        PropBundle.Write(writer);
        PropBundleRanged.Write(writer);
        InitialRtpc.Write(writer);
    }
}
