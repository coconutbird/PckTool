namespace PckTool.Core.WWise.Bnk.Structs;

/// <summary>
///     Track switch params (TrackSwitchInfo::SetSwitchParams) for MusicTrack.
///     Only present when TrackType == 0x3 (Switch).
/// </summary>
public class TrackSwitchParams
{
    /// <summary>
    ///     Group type (AkGroupType).
    /// </summary>
    public byte GroupType { get; set; }

    /// <summary>
    ///     Group ID.
    /// </summary>
    public uint GroupId { get; set; }

    /// <summary>
    ///     Default switch ID.
    /// </summary>
    public uint DefaultSwitch { get; set; }

    /// <summary>
    ///     Switch associations.
    /// </summary>
    public List<uint> SwitchAssociations { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        GroupType = reader.ReadByte();
        GroupId = reader.ReadUInt32();
        DefaultSwitch = reader.ReadUInt32();

        var numSwitchAssoc = reader.ReadUInt32();

        for (var i = 0; i < numSwitchAssoc; i++)
        {
            SwitchAssociations.Add(reader.ReadUInt32());
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(GroupType);
        writer.Write(GroupId);
        writer.Write(DefaultSwitch);

        writer.Write((uint) SwitchAssociations.Count);

        foreach (var switchAssoc in SwitchAssociations)
        {
            writer.Write(switchAssoc);
        }
    }
}
