using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

/// <summary>
///     Bus initial parameters for bank version 113 (v90-122 range in wwiser).
///     Corresponds to CAkBus::SetInitialParams in wwiser.
/// </summary>
public class BusInitialParams
{
    /// <summary>PropBundle is read inside BusInitialParams for v57+ (including v113)</summary>
    public PropBundle PropBundle { get; set; } = null!;

    /// <summary>Bit 0: bMainOutputHierarchy, Bit 1: bIsBackgroundMusic</summary>
    public BusFlags1 Flags1 { get; set; }

    public bool MainOutputHierarchy
    {
        get => Flags1.HasFlag(BusFlags1.MainOutputHierarchy);
        set => Flags1 = value ? Flags1 | BusFlags1.MainOutputHierarchy : Flags1 & ~BusFlags1.MainOutputHierarchy;
    }

    public bool IsBackgroundMusic
    {
        get => Flags1.HasFlag(BusFlags1.IsBackgroundMusic);
        set => Flags1 = value ? Flags1 | BusFlags1.IsBackgroundMusic : Flags1 & ~BusFlags1.IsBackgroundMusic;
    }

    /// <summary>Bit 0: bKillNewest, Bit 1: bUseVirtualBehavior</summary>
    public BusFlags2 Flags2 { get; set; }

    public bool KillNewest
    {
        get => Flags2.HasFlag(BusFlags2.KillNewest);
        set => Flags2 = value ? Flags2 | BusFlags2.KillNewest : Flags2 & ~BusFlags2.KillNewest;
    }

    public bool UseVirtualBehavior
    {
        get => Flags2.HasFlag(BusFlags2.UseVirtualBehavior);
        set => Flags2 = value ? Flags2 | BusFlags2.UseVirtualBehavior : Flags2 & ~BusFlags2.UseVirtualBehavior;
    }

    public ushort MaxNumInstance { get; set; }

    /// <summary>
    ///     Complex bitfield: bits 0-7: uNumChannels, bits 8-11: eConfigType, bits 12-31: uChannelMask
    /// </summary>
    public uint ChannelConfig { get; set; }

    /// <summary>Bit 0: bIsHdrBus, Bit 1: bHdrReleaseModeExponential</summary>
    public BusFlags3 Flags3 { get; set; }

    public bool IsHdrBus
    {
        get => Flags3.HasFlag(BusFlags3.IsHdrBus);
        set => Flags3 = value ? Flags3 | BusFlags3.IsHdrBus : Flags3 & ~BusFlags3.IsHdrBus;
    }

    public bool HdrReleaseModeExponential
    {
        get => Flags3.HasFlag(BusFlags3.HdrReleaseModeExponential);
        set =>
            Flags3 = value
                ? Flags3 | BusFlags3.HdrReleaseModeExponential
                : Flags3 & ~BusFlags3.HdrReleaseModeExponential;
    }

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
        Flags1 = (BusFlags1) reader.ReadByte(); // bMainOutputHierarchy, bIsBackgroundMusic
        Flags2 = (BusFlags2) reader.ReadByte(); // bKillNewest, bUseVirtualBehavior
        MaxNumInstance = reader.ReadUInt16();
        ChannelConfig = reader.ReadUInt32();
        Flags3 = (BusFlags3) reader.ReadByte(); // bIsHdrBus, bHdrReleaseModeExponential

        PropBundle = propBundle;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        PropBundle.Write(writer);
        writer.Write((byte) Flags1);
        writer.Write((byte) Flags2);
        writer.Write(MaxNumInstance);
        writer.Write(ChannelConfig);
        writer.Write((byte) Flags3);
    }
}
