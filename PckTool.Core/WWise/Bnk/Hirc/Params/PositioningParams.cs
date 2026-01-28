using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class PositioningParams
{
    public PositioningFlags Flags { get; set; }

    public bool PositioningInfoOverrideParent
    {
        get => Flags.HasFlag(PositioningFlags.PositioningInfoOverrideParent);
        set =>
            Flags = value
                ? Flags | PositioningFlags.PositioningInfoOverrideParent
                : Flags & ~PositioningFlags.PositioningInfoOverrideParent;
    }

    public bool Enable2D
    {
        get => Flags.HasFlag(PositioningFlags.Enable2D);
        set => Flags = value ? Flags | PositioningFlags.Enable2D : Flags & ~PositioningFlags.Enable2D;
    }

    public bool EnableSpatialization
    {
        get => Flags.HasFlag(PositioningFlags.EnableSpatialization);
        set =>
            Flags = value
                ? Flags | PositioningFlags.EnableSpatialization
                : Flags & ~PositioningFlags.EnableSpatialization;
    }

    public bool Is3DPositioningAvailable
    {
        get => Flags.HasFlag(PositioningFlags.Is3DPositioningAvailable);
        set =>
            Flags = value
                ? Flags | PositioningFlags.Is3DPositioningAvailable
                : Flags & ~PositioningFlags.Is3DPositioningAvailable;
    }

    public Positioning3DFlags? Flags3D { get; set; }

    public byte SpatializationMode
    {
        get => (byte) ((byte) (Flags3D ?? 0) & 0x01); // 1 bit for v113 (v90-v126)
        set
        {
            Is3DPositioningAvailable = true;
            Flags3D ??= Positioning3DFlags.None;
            Flags3D = (Positioning3DFlags) (((byte) Flags3D.Value & 0xFE) | (value & 0x01));
        }
    }

    public bool HoldEmitterPosAndOrient
    {
        get => (Flags3D ?? Positioning3DFlags.None).HasFlag(Positioning3DFlags.HoldEmitterPosAndOrient);
        set
        {
            Is3DPositioningAvailable = true;
            Flags3D ??= Positioning3DFlags.None;
            Flags3D = value
                ? Flags3D.Value | Positioning3DFlags.HoldEmitterPosAndOrient
                : Flags3D.Value & ~Positioning3DFlags.HoldEmitterPosAndOrient;
        }
    }

    public bool HoldListenerOrient
    {
        get => (Flags3D ?? Positioning3DFlags.None).HasFlag(Positioning3DFlags.HoldListenerOrient);
        set
        {
            Is3DPositioningAvailable = true;
            Flags3D ??= Positioning3DFlags.None;
            Flags3D = value
                ? Flags3D.Value | Positioning3DFlags.HoldListenerOrient
                : Flags3D.Value & ~Positioning3DFlags.HoldListenerOrient;
        }
    }

    public uint? AttenuationId { get; set; }

    // 3D Positioning Override Data
    public byte? PathMode { get; set; }
    public int? TransitionTime { get; set; }
    public List<PathVertex>? PathVertices { get; set; }
    public List<PlaylistItem>? PlaylistItems { get; set; }
    public Ak3DAutomationParams? Ak3DAutomationParams { get; set; }

    public bool Read(BinaryReader reader)
    {
        Flags = (PositioningFlags) reader.ReadByte();

        if (Is3DPositioningAvailable)
        {
            Flags3D = (Positioning3DFlags) reader.ReadByte();

            // TODO: verify if AttenuationId is always present when 3D positioning is available
            AttenuationId = reader.ReadUInt32();

            // v113 (v90-v122): has_automation = (e3DPositionType != 1)
            // e3DPositionType is bits 0-1 of Flags3D
            var e3DPositionType = (byte) Flags3D & 0x03;

            if (e3DPositionType != 1)
            {
                var pathMode = reader.ReadByte();
                var transitionTime = reader.ReadInt32();

                var pathVertices = new List<PathVertex>();
                var numberOfVertices = reader.ReadUInt32();

                for (var i = 0; i < numberOfVertices; ++i)
                {
                    var pathVertex = new PathVertex();

                    if (!pathVertex.Read(reader))
                    {
                        return false;
                    }

                    pathVertices.Add(pathVertex);
                }

                var playlistItems = new List<PlaylistItem>();
                var numberOfPlaylistItems = reader.ReadUInt32();

                for (var i = 0; i < numberOfPlaylistItems; ++i)
                {
                    var playlistItem = new PlaylistItem();

                    if (!playlistItem.Read(reader))
                    {
                        return false;
                    }

                    playlistItems.Add(playlistItem);
                }

                var ak3DAutomationParams = new Ak3DAutomationParams();

                if (!ak3DAutomationParams.Read(reader))
                {
                    return false;
                }

                PathMode = pathMode;
                TransitionTime = transitionTime;
                PathVertices = pathVertices;
                PlaylistItems = playlistItems;
                Ak3DAutomationParams = ak3DAutomationParams;
            }
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Flags);

        if (Is3DPositioningAvailable)
        {
            writer.Write((byte) Flags3D!.Value);
            writer.Write(AttenuationId!.Value);

            // e3DPositionType is bits 0-1 of Flags3D
            var e3DPositionType = (byte) Flags3D & 0x03;

            if (e3DPositionType != 1)
            {
                writer.Write(PathMode!.Value);
                writer.Write(TransitionTime!.Value);

                writer.Write((uint) (PathVertices?.Count ?? 0));

                if (PathVertices != null)
                {
                    foreach (var vertex in PathVertices)
                    {
                        vertex.Write(writer);
                    }
                }

                writer.Write((uint) (PlaylistItems?.Count ?? 0));

                if (PlaylistItems != null)
                {
                    foreach (var item in PlaylistItems)
                    {
                        item.Write(writer);
                    }
                }

                Ak3DAutomationParams?.Write(writer);
            }
        }
    }
}
