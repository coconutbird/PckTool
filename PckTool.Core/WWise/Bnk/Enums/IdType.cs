using PckTool.Core.WWise.Bnk.Hirc.Items;

namespace PckTool.Core.WWise.Bnk.Enums;

/// <summary>
///     ID type categories for HIRC objects.
///     Different ID types can share the same numeric ID.
///     Based on wwiser's idtype groupings.
/// </summary>
public enum IdType : byte
{
    /// <summary>
    ///     Events (CAkEvent).
    /// </summary>
    Event,

    /// <summary>
    ///     Dialogue events (CAkDialogueEvent).
    /// </summary>
    DialogueEvent,

    /// <summary>
    ///     Buses (CAkBus, CAkAuxBus, CAkFeedbackBus).
    /// </summary>
    Bus,

    /// <summary>
    ///     Share sets / effects (CAkFxShareSet).
    /// </summary>
    ShareSet,

    /// <summary>
    ///     Audio devices (CAkAudioDevice).
    /// </summary>
    AudioDevice,

    /// <summary>
    ///     Audio objects - default for most HIRC types
    ///     (Sound, Action, ActorMixer, Containers, Music, etc.).
    /// </summary>
    Audio
}

/// <summary>
///     Extension methods for IdType.
/// </summary>
public static class IdTypeExtensions
{
    /// <summary>
    ///     Gets the IdType category for a given HircType.
    /// </summary>
    public static IdType GetIdType(this HircType hircType)
    {
        return hircType switch
        {
            HircType.Event => IdType.Event,
            HircType.DialogueEvent => IdType.DialogueEvent,
            HircType.Bus => IdType.Bus,
            HircType.AuxBus => IdType.Bus,
            HircType.FeedbackBus => IdType.Bus,
            HircType.FxShareSet => IdType.ShareSet,
            HircType.AudioDevice => IdType.AudioDevice,
            _ => IdType.Audio
        };
    }

    public static IdType GetIdType<T>() where T : HircItem
    {
        return typeof(T).GetIdType();
    }

    public static IdType GetIdType(this Type type)
    {
        if (!typeof(HircItem).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type} is not a HircItem");
        }

        return type switch
        {
            _ when type == typeof(EventItem) => IdType.Event,
            _ when type == typeof(DialogueEventItem) => IdType.DialogueEvent,
            _ when type == typeof(BusItem) => IdType.Bus,
            _ when type == typeof(AuxBusItem) => IdType.Bus,
            _ when type == typeof(FeedbackBusItem) => IdType.Bus,
            _ when type == typeof(FxItem) => IdType.ShareSet,
            _ when type == typeof(AudioDeviceItem) => IdType.AudioDevice,
            _ => IdType.Audio
        };
    }
}
