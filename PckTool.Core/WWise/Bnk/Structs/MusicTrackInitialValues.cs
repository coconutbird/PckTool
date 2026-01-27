namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     MusicTrack initial values for bank version 113.
///     Corresponds to CAkMusicTrack::SetInitialValues in wwiser.
/// </summary>
public class MusicTrackInitialValues
{
    /// <summary>
    ///     Override flags (for v90-112: bOverrideParentMidiTempo, bOverrideParentMidiTarget).
    /// </summary>
    public byte Overrides { get; set; }

    /// <summary>
    ///     List of audio sources (AkBankSourceData).
    /// </summary>
    public List<BankSourceData> Sources { get; set; } = [];

    /// <summary>
    ///     Playlist items (AkTrackSrcInfo).
    /// </summary>
    public List<TrackSrcInfo> Playlist { get; set; } = [];

    /// <summary>
    ///     Number of sub-tracks.
    /// </summary>
    public uint NumSubTrack { get; set; }

    /// <summary>
    ///     Clip automation items (AkClipAutomation).
    /// </summary>
    public List<ClipAutomation> ClipAutomationItems { get; set; } = [];

    /// <summary>
    ///     Node base params (inherited from CAkParameterNodeBase).
    /// </summary>
    public NodeBaseParams NodeBaseParams { get; set; } = null!;

    /// <summary>
    ///     Track type (eTrackType for v90+).
    /// </summary>
    public byte TrackType { get; set; }

    /// <summary>
    ///     Switch params (only if TrackType == 0x3).
    /// </summary>
    public TrackSwitchParams? SwitchParams { get; set; }

    /// <summary>
    ///     Transition params (only if TrackType == 0x3).
    /// </summary>
    public TrackTransParams? TransParams { get; set; }

    /// <summary>
    ///     Look-ahead time in milliseconds.
    /// </summary>
    public int LookAheadTime { get; set; }

    public bool Read(BinaryReader reader)
    {
        // For v90-112: uOverrides (u8)
        // bits: 1=bOverrideParentMidiTempo, 2=bOverrideParentMidiTarget
        Overrides = reader.ReadByte();

        // numSources (u32)
        var numSources = reader.ReadUInt32();

        // Read each source (AkBankSourceData)
        for (var i = 0; i < numSources; i++)
        {
            var source = new BankSourceData();

            if (!source.Read(reader))
            {
                return false;
            }

            Sources.Add(source);
        }

        // For v27+: playlist (AkTrackSrcInfo)
        var numPlaylistItem = reader.ReadUInt32();

        if (numPlaylistItem > 0)
        {
            for (var i = 0; i < numPlaylistItem; i++)
            {
                var srcInfo = new TrackSrcInfo();

                if (!srcInfo.Read(reader))
                {
                    return false;
                }

                Playlist.Add(srcInfo);
            }

            NumSubTrack = reader.ReadUInt32();
        }

        // For v65+: clip automation items (AkClipAutomation)
        var numClipAutomationItem = reader.ReadUInt32();

        for (var i = 0; i < numClipAutomationItem; i++)
        {
            var clipAuto = new ClipAutomation();

            if (!clipAuto.Read(reader))
            {
                return false;
            }

            ClipAutomationItems.Add(clipAuto);
        }

        // NodeBaseParams
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;

        // For v90+: eTrackType (u8)
        TrackType = reader.ReadByte();

        // If TrackType == 0x3 (Switch), read switch and trans params
        if (TrackType == 0x3)
        {
            var switchParams = new TrackSwitchParams();

            if (!switchParams.Read(reader))
            {
                return false;
            }

            SwitchParams = switchParams;

            var transParams = new TrackTransParams();

            if (!transParams.Read(reader))
            {
                return false;
            }

            TransParams = transParams;
        }

        // iLookAheadTime (s32)
        LookAheadTime = reader.ReadInt32();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Overrides);

        writer.Write((uint) Sources.Count);

        foreach (var source in Sources)
        {
            source.Write(writer);
        }

        writer.Write((uint) Playlist.Count);

        if (Playlist.Count > 0)
        {
            foreach (var srcInfo in Playlist)
            {
                srcInfo.Write(writer);
            }

            writer.Write(NumSubTrack);
        }

        writer.Write((uint) ClipAutomationItems.Count);

        foreach (var clipAuto in ClipAutomationItems)
        {
            clipAuto.Write(writer);
        }

        NodeBaseParams.Write(writer);
        writer.Write(TrackType);

        if (TrackType == 0x3)
        {
            SwitchParams?.Write(writer);
            TransParams?.Write(writer);
        }

        writer.Write(LookAheadTime);
    }
}
