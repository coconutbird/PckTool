namespace SoundsUnpack.WWise.Structs;

public class PositioningParams
{
    public byte BitVector { get; set; }

    public bool PositioningInfoOverrideParent
    {
        get => (BitVector & 0x01) != 0;
        set
        {
            if (value)
                BitVector |= 0x01;
            else
                BitVector &= 0xFE; // 11111110
        }
    }

    public bool Enable2D
    {
        get => (BitVector & 0x02) != 0;
        set
        {
            if (value)
                BitVector |= 0x02;
            else
                BitVector &= 0xFD; // 11111101
        }
    }

    public bool EnableSpatialization
    {
        get => (BitVector & 0x04) != 0;
        set
        {
            if (value)
                BitVector |= 0x04;
            else
                BitVector &= 0xFB; // 11111011
        }
    }

    public bool Is3DPositioningAvailable
    {
        get => (BitVector & 0x08) != 0;
        set
        {
            if (value)
                BitVector |= 0x08;
            else
                BitVector &= 0xF7;
        }
    }

    public byte? Bits3D { get; set; }

    public byte SpatializationMode
    {
        get => (byte)(Bits3D & 0x03)!;
        set
        {
            Is3DPositioningAvailable = true;
            Bits3D ??= 0;
            Bits3D &= 0xFC;
            Bits3D |= (byte)(value & 0x03);
        }
    }

    public bool HoldEmitterPosAndOrient
    {
        get => (Bits3D & 0x08) != 0;
        set
        {
            Is3DPositioningAvailable = true;
            Bits3D ??= 0;
            if (value)
                Bits3D |= 0x08;
            else
                Bits3D &= 0xF7;
        }
    }

    public bool HoldListenerOrient
    {
        get => (Bits3D & 0x10) != 0;
        set
        {
            Is3DPositioningAvailable = true;
            Bits3D ??= 0;
            if (value)
                Bits3D |= 0x10;
            else
                Bits3D &= 0xEF;
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
        BitVector = reader.ReadByte();

        if (Is3DPositioningAvailable)
        {
            Bits3D = reader.ReadByte();

            // TODO: verify if AttenuationId is always present when 3D positioning is available
            AttenuationId = reader.ReadUInt32();

            if (PositioningInfoOverrideParent && AttenuationId == 0)
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
}