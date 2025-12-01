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
    
    // CAkActionPlay
    public ushort? ActionType { get; set; }
    public ActionInitialValues? ActionInitialValues { get; set; }
    
    // CAkEvent
    public EventInitialValues? EventInitialValues { get; set; }

    public static int Idx = 0;

    public bool Read(BinaryReader reader)
    {
        Type = (HircType)reader.ReadByte();

        var sectionSize = reader.ReadUInt32();

        Id = reader.ReadUInt32();

        Console.WriteLine($"Reading HIRC Item: Type={Type}, Id={Id}, Size={sectionSize}, Idx={Idx}");

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

            default:
                return false;
        }

        Idx++;

        return true;
    }
}