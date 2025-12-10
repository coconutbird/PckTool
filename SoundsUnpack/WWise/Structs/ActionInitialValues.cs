using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Helpers;

namespace SoundsUnpack.WWise.Structs;

public class ActionInitialValues
{
    public uint Ext { get; set; }
    public byte Ext4 { get; set; }

    public bool IsBus
    {
        get => (Ext4 & 0x01) != 0;
        set
        {
            if (value)
            {
                Ext4 |= 0x01;
            }
            else
            {
                Ext4 &= 0xFE;
            }
        }
    }

    public PropBundle PropBundle1 { get; set; }
    public PropBundle PropBundle2 { get; set; }
    public PlayActionParams? PlayActionParams { get; set; }
    public ValueActionParams? ValueActionParams { get; set; }
    public ActiveActionParams? ActiveActionParams { get; set; }

    public bool Read(BinaryReader reader, ushort actionType)
    {
        var ext = reader.ReadUInt32();
        var ext4 = reader.ReadByte();

        var propBundle1 = new PropBundle();

        if (!propBundle1.Read(reader))
        {
            return false;
        }

        var propBundle2 = new PropBundle();

        if (!propBundle2.Read(reader, true))
        {
            return false;
        }

        var actionCategory = ActionTypeHelpers.GetActionCategory((ActionType) actionType);

        switch (actionCategory)
        {
            case ActionCategory.Play:
            {
                var playActionParams = new PlayActionParams();

                if (!playActionParams.Read(reader))
                {
                    return false;
                }

                PlayActionParams = playActionParams;

                break;
            }

            case ActionCategory.Value:
            {
                var valueActionParams = new ValueActionParams();

                if (!valueActionParams.Read(reader))
                {
                    return false;
                }

                ValueActionParams = valueActionParams;

                break;
            }

            case ActionCategory.Active:
            {
                var activeActionParams = new ActiveActionParams();

                if (!activeActionParams.Read(reader, (ActionType) actionType))
                {
                    return false;
                }

                ActiveActionParams = activeActionParams;

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(actionCategory), "Unsupported action category");
        }

        Ext = ext;
        Ext4 = ext4;
        PropBundle1 = propBundle1;
        PropBundle2 = propBundle2;

        return true;
    }
}
