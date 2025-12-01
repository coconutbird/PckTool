using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Structs;

public class LoadedItem
{
    public HircType Type { get; set; }
    public uint Id { get; set; }

    public AttenuationInitialValues? AttenuationValues { get; set; }
    public SoundInitialValues? SoundValues { get; set; }

    public bool Read(BinaryReader reader)
    {
        Type = (HircType)reader.ReadByte();

        var sectionSIze = reader.ReadUInt32();

        Id = reader.ReadUInt32();

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

            default:
                return false;
        }

        return true;
    }
}