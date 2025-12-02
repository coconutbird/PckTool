namespace SoundsUnpack.WWise.Structs;

public class BusInitialValues
{
    public uint OverrideBusId { get; set; }
    public PropBundle PropBundle { get; set; }
    public BusInitialParams BusInitialParams { get; set; }
    public int RecoveryTime { get; set; }

    public float MaxDuckVolume { get; set; }

    // TODO: Implement Duck class
    public List<object> Ducks { get; set; } = new();
    public BusInitialFxParams BusInitialFxParams { get; set; }
    public bool OverrideAttachmentParams { get; set; }
    public InitialRtpc InitialRtpc { get; set; }
    public StateChunk StateChunk { get; set; }

    public bool Read(BinaryReader reader)
    {
        var overrideBusId = reader.ReadUInt32();

        var propBundle = new PropBundle();
        if (!propBundle.Read(reader))
        {
            return false;
        }

        var busInitialParams = new BusInitialParams();
        if (!busInitialParams.Read(reader))
        {
            return false;
        }

        var recoveryTime = reader.ReadInt32();
        var maxDucksVolume = reader.ReadSingle();

        var numberOfDucks = reader.ReadUInt32();
        if (numberOfDucks > 0)
        {
            throw new NotImplementedException("BusInitialValues: numberOfDucks > 0 not implemented");
        }

        var busInitialFxParams = new BusInitialFxParams();
        if (!busInitialFxParams.Read(reader))
        {
            return false;
        }

        var overrideAttachmentParams = reader.ReadByte() != 0;
        if (overrideAttachmentParams)
        {
            throw new NotImplementedException("BusInitialValues: overrideAttachmentParams not implemented");
        }

        var initialRtpc = new InitialRtpc();
        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        var stateChunk = new StateChunk();
        if (!stateChunk.Read(reader))
        {
            return false;
        }

        OverrideBusId = overrideBusId;
        PropBundle = propBundle;
        BusInitialParams = busInitialParams;
        RecoveryTime = recoveryTime;
        MaxDuckVolume = maxDucksVolume;
        BusInitialFxParams = busInitialFxParams;
        OverrideAttachmentParams = overrideAttachmentParams;
        InitialRtpc = initialRtpc;
        StateChunk = stateChunk;

        return true;
    }
}