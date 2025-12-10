namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Bus initial parameters for bank version 113 (v90-122 range in wwiser).
///     Corresponds to CAkBus::SetInitialParams in wwiser.
/// </summary>
public class BusInitialParams
{
    /// <summary>PropBundle is read inside BusInitialParams for v57+ (including v113)</summary>
    public PropBundle PropBundle { get; set; } = null!;

    /// <summary>Bit 0: bMainOutputHierarchy, Bit 1: bIsBackgroundMusic</summary>
    public byte BitVector1 { get; set; }

    /// <summary>Bit 0: bKillNewest, Bit 1: bUseVirtualBehavior</summary>
    public byte BitVector2 { get; set; }

    public ushort MaxNumInstance { get; set; }

    /// <summary>
    ///     Complex bitfield: bits 0-7: uNumChannels, bits 8-11: eConfigType, bits 12-31: uChannelMask
    /// </summary>
    public uint ChannelConfig { get; set; }

    /// <summary>Bit 0: bIsHdrBus, Bit 1: bHdrReleaseModeExponential</summary>
    public byte BitVector3 { get; set; }

    public bool Read(BinaryReader reader)
    {
        // v113 falls in the 90-122 range in wwiser
        // See CAkBus__SetInitialParams in wparser.py

        // For v57+, PropBundle is read inside BusInitialParams
        var propBundle = new PropBundle();

        if (!propBundle.Read(reader))
        {
            return false;
        }

        // For v90-122:
        var bitVector1 = reader.ReadByte(); // bMainOutputHierarchy, bIsBackgroundMusic
        var bitVector2 = reader.ReadByte(); // bKillNewest, bUseVirtualBehavior
        var maxNumInstance = reader.ReadUInt16();
        var channelConfig = reader.ReadUInt32();
        var bitVector3 = reader.ReadByte(); // bIsHdrBus, bHdrReleaseModeExponential

        PropBundle = propBundle;
        BitVector1 = bitVector1;
        BitVector2 = bitVector2;
        MaxNumInstance = maxNumInstance;
        ChannelConfig = channelConfig;
        BitVector3 = bitVector3;

        return true;
    }
}
