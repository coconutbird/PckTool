using PckTool.Abstractions;
using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     Base class for all HIRC (Hierarchy) items.
///     Use pattern matching to access type-specific properties.
/// </summary>
public abstract class HircItem : IHircItem
{
  /// <inheritdoc />
  byte IHircItem.Type => (byte) Type;

  /// <summary>
  ///     The unique ID of this item within the soundbank.
  /// </summary>
  public uint Id { get; protected init; }

  /// <summary>
  ///     The HIRC type of this item.
  /// </summary>
  public abstract HircType Type { get; }

  /// <summary>
  ///     Writes this HIRC item to the writer (including type, size, and ID header).
  /// </summary>
  /// <param name="writer">The binary writer.</param>
  public void Write(BinaryWriter writer)
  {
    // Write type
    writer.Write((byte) Type);

    // Reserve space for size
    var sizePosition = writer.BaseStream.Position;
    writer.Write(0u);

    var contentStart = writer.BaseStream.Position;

    // Write ID
    writer.Write(Id);

    // Write item-specific content
    WriteInternal(writer);

    // Calculate and write size (includes ID)
    var contentEnd = writer.BaseStream.Position;
    var size = (uint) (contentEnd - contentStart);

    writer.BaseStream.Position = sizePosition;
    writer.Write(size);
    writer.BaseStream.Position = contentEnd;
  }

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

    // Remaining size after reading the ID (4 bytes)
    var remainingSize = (int) (sectionSize - 4);

    // Log.Info($"Reading HIRC item: Type {type}, Size {sectionSize}, ID {id}");

    HircItem? item = type switch
    {
      // Fully implemented parsers
      HircType.Attenuation => AttenuationItem.ReadItem(reader, id),
      HircType.Sound => SoundItem.ReadItem(reader, id),
      HircType.RanSeqCntr => RanSeqCntrItem.ReadItem(reader, id),
      HircType.ActorMixer => ActorMixerItem.ReadItem(reader, id),
      HircType.Bus => BusItem.ReadItem(reader, id),
      HircType.Action => ActionItem.ReadItem(reader, id),
      HircType.Event => EventItem.ReadItem(reader, id),
      HircType.FxShareSet or HircType.FxCustom => FxItem.ReadItem(reader, id, type),

      // Stub implementations (raw bytes preserved)
      HircType.State => StateItem.ReadItem(reader, id, remainingSize),
      HircType.SwitchCntr => SwitchCntrItem.ReadItem(reader, id, remainingSize),
      HircType.LayerCntr => LayerCntrItem.ReadItem(reader, id, remainingSize),
      HircType.Segment => MusicSegmentItem.ReadItem(reader, id),
      HircType.Track => MusicTrackItem.ReadItem(reader, id),
      HircType.MusicSwitch => MusicSwitchItem.ReadItem(reader, id),
      HircType.MusicRanSeq => MusicRanSeqItem.ReadItem(reader, id),
      HircType.DialogueEvent => DialogueEventItem.ReadItem(reader, id, remainingSize),
      HircType.FeedbackBus => FeedbackBusItem.ReadItem(reader, id, remainingSize),
      HircType.FeedbackNode => FeedbackNodeItem.ReadItem(reader, id, remainingSize),
      HircType.AuxBus => AuxBusItem.ReadItem(reader, id, remainingSize),
      HircType.LfoModulator => LfoModulatorItem.ReadItem(reader, id, remainingSize),
      HircType.EnvelopeModulator => EnvelopeModulatorItem.ReadItem(reader, id, remainingSize),
      HircType.AudioDevice => AudioDeviceItem.ReadItem(reader, id, remainingSize),

      _ => null
    };

    if (item is null)
    {
      Log.Error($"Failed to parse HIRC item: Type {type}, Size {sectionSize}, ID {id}");

      return null;
    }

    var expectedPosition = baseOffset + sectionSize;

    if (reader.BaseStream.Position != expectedPosition)
    {
      Log.Error(
        $"HIRC item read position mismatch for type {type}. Expected {expectedPosition}, got {reader.BaseStream.Position}.");

      // Seek to correct position to allow parsing to continue
      reader.BaseStream.Position = expectedPosition;
    }

    return item;
  }

  /// <summary>
  ///     Implement this method to write item-specific data (after the ID).
  /// </summary>
  /// <param name="writer">The binary writer.</param>
  protected virtual void WriteInternal(BinaryWriter writer)
  {
    throw new NotImplementedException($"Write is not implemented for HIRC item type {Type}.");
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
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

    try
    {
      if (!values.Read(reader, actionType)) return null;
    }
    catch (Exception e)
    {
      Log.Error(e, $"Failed to parse action item {id} with action type {actionType}");

      return null;
    }

    return new ActionItem { Id = id, ActionType = actionType, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    writer.Write((ushort) ActionType);
    Values.Write(writer, ActionType);
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
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

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

#region Implemented HIRC Items (formerly stubs)

/// <summary>
///     Represents a CAkState HIRC item.
/// </summary>
public sealed class StateItem : HircItem
{
  public override HircType Type => HircType.State;
  public required StateInitialValues Values { get; init; }

  internal static StateItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new StateInitialValues();

    if (!values.Read(reader)) return null;

    return new StateItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkSwitchCntr HIRC item.
/// </summary>
public sealed class SwitchCntrItem : HircItem
{
  public override HircType Type => HircType.SwitchCntr;
  public required SwitchCntrInitialValues Values { get; init; }

  internal static SwitchCntrItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new SwitchCntrInitialValues();

    if (!values.Read(reader)) return null;

    return new SwitchCntrItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkLayerCntr HIRC item.
/// </summary>
public sealed class LayerCntrItem : HircItem
{
  public override HircType Type => HircType.LayerCntr;
  public required LayerCntrInitialValues Values { get; init; }

  internal static LayerCntrItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new LayerCntrInitialValues();

    if (!values.Read(reader)) return null;

    return new LayerCntrItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkMusicSegment HIRC item.
/// </summary>
public sealed class MusicSegmentItem : HircItem
{
  public override HircType Type => HircType.Segment;
  public required MusicSegmentInitialValues Values { get; init; }

  internal static MusicSegmentItem? ReadItem(BinaryReader reader, uint id)
  {
    var values = new MusicSegmentInitialValues();

    if (!values.Read(reader)) return null;

    return new MusicSegmentItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkMusicTrack HIRC item.
/// </summary>
public sealed class MusicTrackItem : HircItem
{
  public override HircType Type => HircType.Track;
  public required MusicTrackInitialValues Values { get; init; }

  internal static MusicTrackItem? ReadItem(BinaryReader reader, uint id)
  {
    var values = new MusicTrackInitialValues();

    if (!values.Read(reader)) return null;

    return new MusicTrackItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkMusicSwitchCntr HIRC item.
/// </summary>
public sealed class MusicSwitchItem : HircItem
{
  public override HircType Type => HircType.MusicSwitch;
  public required MusicSwitchCntrInitialValues Values { get; init; }

  internal static MusicSwitchItem? ReadItem(BinaryReader reader, uint id)
  {
    var values = new MusicSwitchCntrInitialValues();

    if (!values.Read(reader)) return null;

    return new MusicSwitchItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkMusicRanSeqCntr HIRC item.
/// </summary>
public sealed class MusicRanSeqItem : HircItem
{
  public override HircType Type => HircType.MusicRanSeq;
  public required MusicRanSeqCntrInitialValues Values { get; init; }

  internal static MusicRanSeqItem? ReadItem(BinaryReader reader, uint id)
  {
    var values = new MusicRanSeqCntrInitialValues();

    if (!values.Read(reader)) return null;

    return new MusicRanSeqItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkDialogueEvent HIRC item.
/// </summary>
public sealed class DialogueEventItem : HircItem
{
  public override HircType Type => HircType.DialogueEvent;
  public required DialogueEventInitialValues Values { get; init; }

  internal static DialogueEventItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new DialogueEventInitialValues();

    if (!values.Read(reader)) return null;

    return new DialogueEventItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkFeedbackBus HIRC item.
///     Note: FeedbackBus is deprecated in newer Wwise versions (v125+).
/// </summary>
public sealed class FeedbackBusItem : HircItem
{
  public override HircType Type => HircType.FeedbackBus;
  public required BusInitialValues Values { get; init; }

  internal static FeedbackBusItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new BusInitialValues();

    if (!values.Read(reader)) return null;

    return new FeedbackBusItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkFeedbackNode HIRC item.
///     Note: FeedbackNode is deprecated in newer Wwise versions (v125+).
/// </summary>
public sealed class FeedbackNodeItem : HircItem
{
  public override HircType Type => HircType.FeedbackNode;
  public required FeedbackNodeInitialValues Values { get; init; }

  internal static FeedbackNodeItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new FeedbackNodeInitialValues();

    if (!values.Read(reader)) return null;

    return new FeedbackNodeItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkAuxBus HIRC item.
/// </summary>
public sealed class AuxBusItem : HircItem
{
  public override HircType Type => HircType.AuxBus;
  public required BusInitialValues Values { get; init; }

  internal static AuxBusItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new BusInitialValues();

    if (!values.Read(reader)) return null;

    return new AuxBusItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkLFOModulator HIRC item.
/// </summary>
public sealed class LfoModulatorItem : HircItem
{
  public override HircType Type => HircType.LfoModulator;
  public required ModulatorInitialValues Values { get; init; }

  internal static LfoModulatorItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new ModulatorInitialValues();

    if (!values.Read(reader)) return null;

    return new LfoModulatorItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkEnvelopeModulator HIRC item.
/// </summary>
public sealed class EnvelopeModulatorItem : HircItem
{
  public override HircType Type => HircType.EnvelopeModulator;
  public required ModulatorInitialValues Values { get; init; }

  internal static EnvelopeModulatorItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new ModulatorInitialValues();

    if (!values.Read(reader)) return null;

    return new EnvelopeModulatorItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

/// <summary>
///     Represents a CAkAudioDevice HIRC item.
/// </summary>
public sealed class AudioDeviceItem : HircItem
{
  public override HircType Type => HircType.AudioDevice;
  public required FxBaseInitialValues Values { get; init; }

  internal static AudioDeviceItem? ReadItem(BinaryReader reader, uint id, int remainingSize)
  {
    var values = new FxBaseInitialValues();

    if (!values.Read(reader)) return null;

    return new AudioDeviceItem { Id = id, Values = values };
  }

  protected override void WriteInternal(BinaryWriter writer)
  {
    Values.Write(writer);
  }
}

#endregion
