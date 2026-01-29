using PckTool.Core.WWise.Bnk.Hirc.Params;

namespace PckTool.Core.WWise.Bnk.Hirc.Items;

/// <summary>
///     Bus initial values for bank version 113 (v90-122 range in wwiser).
///     Corresponds to CAkBus::SetInitialValues in wwiser.
/// </summary>
public class BusInitialValues
{
    public uint OverrideBusId { get; set; }
    public BusInitialParams BusInitialParams { get; set; } = null!;
    public int RecoveryTime { get; set; }
    public float MaxDuckVolume { get; set; }
    public List<DuckInfo> Ducks { get; set; } = [];
    public BusInitialFxParams BusInitialFxParams { get; set; } = null!;
    public byte OverrideAttachmentParams { get; set; }
    public InitialRtpc InitialRtpc { get; set; } = null!;
    public StateChunk StateChunk { get; set; } = null!;

    public bool Read(BinaryReader reader)
    {
        // See CAkBus__SetInitialValues in wparser.py

        // 1. OverrideBusId
        var overrideBusId = reader.ReadUInt32();

        // 2. BusInitialParams (includes PropBundle for v57+)
        var busInitialParams = new BusInitialParams();

        if (!busInitialParams.Read(reader))
        {
            return false;
        }

        // 3. RecoveryTime
        var recoveryTime = reader.ReadInt32();

        // 4. MaxDuckVolume (v39+)
        var maxDuckVolume = reader.ReadSingle();

        // 5. Duck list
        var numDucks = reader.ReadUInt32();
        var ducks = new List<DuckInfo>();

        for (var i = 0; i < numDucks; i++)
        {
            var duck = new DuckInfo();

            if (!duck.Read(reader))
            {
                return false;
            }

            ducks.Add(duck);
        }

        // 6. BusInitialFxParams
        var busInitialFxParams = new BusInitialFxParams();

        if (!busInitialFxParams.Read(reader))
        {
            return false;
        }

        // 7. bOverrideAttachmentParams (v90-145)
        var overrideAttachmentParams = reader.ReadByte();

        // 8. InitialRTPC
        var initialRtpc = new InitialRtpc();

        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        // 9. StateChunk (v53-122 uses CAkParameterNodeBase__ReadStateChunk)
        var stateChunk = new StateChunk();

        if (!stateChunk.Read(reader))
        {
            return false;
        }

        // 10. FeedbackInfo (v<=126) - for v113 this is read
        // See CAkParameterNodeBase__ReadFeedbackInfo - but typically empty for Bus

        OverrideBusId = overrideBusId;
        BusInitialParams = busInitialParams;
        RecoveryTime = recoveryTime;
        MaxDuckVolume = maxDuckVolume;
        Ducks = ducks;
        BusInitialFxParams = busInitialFxParams;
        OverrideAttachmentParams = overrideAttachmentParams;
        InitialRtpc = initialRtpc;
        StateChunk = stateChunk;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(OverrideBusId);
        BusInitialParams.Write(writer);
        writer.Write(RecoveryTime);
        writer.Write(MaxDuckVolume);

        writer.Write((uint) Ducks.Count);

        foreach (var duck in Ducks)
        {
            duck.Write(writer);
        }

        BusInitialFxParams.Write(writer);
        writer.Write(OverrideAttachmentParams);
        InitialRtpc.Write(writer);
        StateChunk.Write(writer);
    }
}

/// <summary>
///     Duck info structure (AkDuckInfo).
///     Corresponds to ToDuckList elements in wwiser.
/// </summary>
public class DuckInfo
{
    public uint BusId { get; set; }
    public float DuckVolume { get; set; }
    public int FadeOutTime { get; set; }
    public int FadeInTime { get; set; }
    public byte FadeCurve { get; set; }
    public byte TargetProp { get; set; } // v66+

    public bool Read(BinaryReader reader)
    {
        BusId = reader.ReadUInt32();
        DuckVolume = reader.ReadSingle();
        FadeOutTime = reader.ReadInt32();
        FadeInTime = reader.ReadInt32();
        FadeCurve = reader.ReadByte();
        TargetProp = reader.ReadByte(); // v66+ (v113 qualifies)

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(BusId);
        writer.Write(DuckVolume);
        writer.Write(FadeOutTime);
        writer.Write(FadeInTime);
        writer.Write(FadeCurve);
        writer.Write(TargetProp);
    }
}
