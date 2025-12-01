using SoundsUnpack.WWise.Enums;

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

        var actionCategory = GetActionCategory(actionType);
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

            case ActionCategory.Set:
            case ActionCategory.Reset:
            {
                var valueActionParams = new ValueActionParams();
                if (!valueActionParams.Read(reader))
                {
                    return false;
                }

                ValueActionParams = valueActionParams;

                break;
            }

            case ActionCategory.Stop:
            {
                var activeActionParams = new ActiveActionParams();
                if (!activeActionParams.Read(reader))
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

    private static ActionCategory GetActionCategory(ushort actionType)
    {
        if (actionType >= 0x400 && actionType <= 0x499)
        {
            return ActionCategory.Play;
        }

        if (actionType >= 0x800 && actionType <= 0x899)
        {
            return ActionCategory.Set;
        }

        if (actionType >= 0x900 && actionType <= 0x999)
        {
            return ActionCategory.Reset;
        }

        if (actionType >= 0x100 && actionType <= 0x199)
        {
            return ActionCategory.Stop;
        }

        throw new ArgumentOutOfRangeException(nameof(actionType), "Unknown action type");
    }
}