using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Base class for all HIRC (Hierarchy) items.
///     Use pattern matching to access type-specific properties.
/// </summary>
public abstract class HircItem
{
    /// <summary>
    ///     The unique ID of this item within the soundbank.
    /// </summary>
    public uint Id { get; protected init; }

    /// <summary>
    ///     The HIRC type of this item.
    /// </summary>
    public abstract HircType Type { get; }

    /// <summary>
    ///     Factory method to read and create the appropriate HircItem subtype.
    /// </summary>
    /// <param name="reader">The binary reader positioned at the start of the HIRC item.</param>
    /// <returns>The parsed HircItem, or null if parsing failed.</returns>
    public static HircItem? Read(BinaryReader reader)
    {
        var type = (HircType) reader.ReadByte();
        var sectionSize = reader.ReadUInt32();
        var baseOffset = reader.BaseStream.Position;
        var id = reader.ReadUInt32();

        Console.WriteLine($"Loading HIRC Item - Type: {type}, ID: {id}");

        HircItem? item = type switch
        {
            HircType.Attenuation => AttenuationItem.ReadItem(reader, id),
            HircType.Sound => SoundItem.ReadItem(reader, id),
            HircType.RanSeqCntr => RanSeqCntrItem.ReadItem(reader, id),
            HircType.ActorMixer => ActorMixerItem.ReadItem(reader, id),
            HircType.Bus => BusItem.ReadItem(reader, id),
            HircType.Action => ActionItem.ReadItem(reader, id),
            HircType.Event => EventItem.ReadItem(reader, id),
            HircType.FxShareSet or HircType.FxCustom => FxItem.ReadItem(reader, id, type),
            _ => null
        };

        if (item is null)
        {
            Console.WriteLine("Unsupported or failed HIRC type: " + type);

            return null;
        }

        var expectedPosition = baseOffset + sectionSize;

        if (reader.BaseStream.Position != expectedPosition)
        {
            Console.WriteLine(
                $"Warning: HircItem read position mismatch for type {type}. Expected {expectedPosition}, got {reader.BaseStream.Position}.");
        }

        return item;
    }
}

/// <summary>
///     Represents a CAkAttenuation HIRC item.
/// </summary>
public sealed class AttenuationItem : HircItem
{
    public override HircType Type => HircType.Attenuation;
    public required AttenuationInitialValues Values { get; init; }

    internal static AttenuationItem? ReadItem(BinaryReader reader, uint id)
    {
        var values = new AttenuationInitialValues();

        if (!values.Read(reader)) return null;

        return new AttenuationItem { Id = id, Values = values };
    }
}

/// <summary>
///     Represents a CAkSound HIRC item.
/// </summary>
public sealed class SoundItem : HircItem
{
    public override HircType Type => HircType.Sound;
    public required SoundInitialValues Values { get; init; }

    internal static SoundItem? ReadItem(BinaryReader reader, uint id)
    {
        var values = new SoundInitialValues();

        if (!values.Read(reader)) return null;

        return new SoundItem { Id = id, Values = values };
    }
}

/// <summary>
///     Represents a CAkRanSeqCntr (Random/Sequence Container) HIRC item.
/// </summary>
public sealed class RanSeqCntrItem : HircItem
{
    public override HircType Type => HircType.RanSeqCntr;
    public required RanSeqCntrInitialValues Values { get; init; }

    internal static RanSeqCntrItem? ReadItem(BinaryReader reader, uint id)
    {
        var values = new RanSeqCntrInitialValues();

        if (!values.Read(reader)) return null;

        return new RanSeqCntrItem { Id = id, Values = values };
    }
}

/// <summary>
///     Represents a CAkActorMixer HIRC item.
/// </summary>
public sealed class ActorMixerItem : HircItem
{
    public override HircType Type => HircType.ActorMixer;
    public required ActorMixerInitialValues Values { get; init; }

    internal static ActorMixerItem? ReadItem(BinaryReader reader, uint id)
    {
        var values = new ActorMixerInitialValues();

        if (!values.Read(reader)) return null;

        return new ActorMixerItem { Id = id, Values = values };
    }
}

/// <summary>
///     Represents a CAkBus HIRC item.
/// </summary>
public sealed class BusItem : HircItem
{
    public override HircType Type => HircType.Bus;
    public required BusInitialValues Values { get; init; }

    internal static BusItem? ReadItem(BinaryReader reader, uint id)
    {
        var values = new BusInitialValues();

        if (!values.Read(reader)) return null;

        return new BusItem { Id = id, Values = values };
    }
}

/// <summary>
///     Represents a CAkAction HIRC item.
/// </summary>
public sealed class ActionItem : HircItem
{
    public override HircType Type => HircType.Action;
    public required ActionType ActionType { get; init; }
    public required ActionInitialValues Values { get; init; }

    internal static ActionItem? ReadItem(BinaryReader reader, uint id)
    {
        var actionType = (ActionType) reader.ReadUInt16();
        var values = new ActionInitialValues();

        if (!values.Read(reader, actionType)) return null;

        return new ActionItem { Id = id, ActionType = actionType, Values = values };
    }
}

/// <summary>
///     Represents a CAkEvent HIRC item.
/// </summary>
public sealed class EventItem : HircItem
{
    public override HircType Type => HircType.Event;
    public required EventInitialValues Values { get; init; }

    internal static EventItem? ReadItem(BinaryReader reader, uint id)
    {
        var values = new EventInitialValues();

        if (!values.Read(reader)) return null;

        return new EventItem { Id = id, Values = values };
    }
}

/// <summary>
///     Represents a CAkFxCustom or CAkFxShareSet HIRC item.
/// </summary>
public sealed class FxItem : HircItem
{
    private FxItem(HircType type)
    {
        Type = type;
    }

    public override HircType Type { get; }

    public required FxBaseInitialValues Values { get; init; }

    internal static FxItem? ReadItem(BinaryReader reader, uint id, HircType type)
    {
        var values = new FxBaseInitialValues();

        if (!values.Read(reader)) return null;

        return new FxItem(type) { Id = id, Values = values };
    }
}
