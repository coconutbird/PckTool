using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Structs;

public class LoadedItem
{
    public HircType Type { get; set; }
    public uint Id { get; set; }

    // CAkAttenuation
    public AttenuationInitialValues? AttenuationValues { get; set; }

    // CAkSound
    public SoundInitialValues? SoundValues { get; set; }

    // CAkRanSeqCntr
    public RanSeqCntrInitialValues? RanSeqCntrInitialValues { get; set; }

    // CAkActorMixer
    public ActorMixerInitialValues? ActorMixerInitialValues { get; set; }

    // CAkBus
    public BusInitialValues? BusInitialValues { get; set; }

    // CAkAction*
    public ushort? ActionType { get; set; }
    public ActionInitialValues? ActionInitialValues { get; set; }

    // CAkEvent
    public EventInitialValues? EventInitialValues { get; set; }

    // CAkFxCustom
    public FxBaseInitialValues? FxBaseInitialValues { get; set; }

    public bool Read(BinaryReader reader)
    {
        Type = (HircType) reader.ReadByte();

        var sectionSize = reader.ReadUInt32();

        var baseOffset = reader.BaseStream.Position;

        Id = reader.ReadUInt32();

        Console.WriteLine($"Loading HIRC Item - Type: {Type}, ID: {Id}");

        switch (Type)
        {
            case HircType.Attenuation:
            {
                var attenuationValues = new AttenuationInitialValues();

                if (!attenuationValues.Read(reader))
                {
                    return false;
                }

                AttenuationValues = attenuationValues;

                break;
            }

            case HircType.Sound:
            {
                var soundValues = new SoundInitialValues();

                if (!soundValues.Read(reader))
                {
                    return false;
                }

                SoundValues = soundValues;

                break;
            }

            case HircType.RanSeqCntr:
            {
                var ranSeqCntrInitialValues = new RanSeqCntrInitialValues();

                if (!ranSeqCntrInitialValues.Read(reader))
                {
                    return false;
                }

                RanSeqCntrInitialValues = ranSeqCntrInitialValues;

                break;
            }

            case HircType.ActorMixer:
            {
                var actorMixerInitialValues = new ActorMixerInitialValues();

                if (!actorMixerInitialValues.Read(reader))
                {
                    return false;
                }

                ActorMixerInitialValues = actorMixerInitialValues;

                break;
            }

            case HircType.Bus:
            {
                var busInitialValues = new BusInitialValues();

                if (!busInitialValues.Read(reader))
                {
                    return false;
                }

                BusInitialValues = busInitialValues;

                break;
            }

            case HircType.Action:
            {
                var actionType = reader.ReadUInt16();
                var actionInitialValues = new ActionInitialValues();

                if (!actionInitialValues.Read(reader, actionType))
                {
                    return false;
                }

                ActionType = actionType;
                ActionInitialValues = actionInitialValues;

                break;
            }

            case HircType.Event:
            {
                var eventInitialValues = new EventInitialValues();

                if (!eventInitialValues.Read(reader))
                {
                    return false;
                }

                EventInitialValues = eventInitialValues;

                break;
            }

            case HircType.FxShareSet:
            case HircType.FxCustom:
            {
                var fxBaseInitialValues = new FxBaseInitialValues();

                if (!fxBaseInitialValues.Read(reader))
                {
                    return false;
                }

                FxBaseInitialValues = fxBaseInitialValues;

                break;
            }

            default:
                Console.WriteLine("Unsupported HIRC type: " + Type);

                return false;
        }

        var expectedPosition = baseOffset + sectionSize;

        if (reader.BaseStream.Position != expectedPosition)
        {
            Console.WriteLine(
                $"Warning: LoadedItem read position mismatch for type {Type}. Expected {expectedPosition}, got {reader.BaseStream.Position}.");
        }

        return true;
    }
}
