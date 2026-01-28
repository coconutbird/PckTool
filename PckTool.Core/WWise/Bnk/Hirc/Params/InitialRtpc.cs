namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class InitialRtpc
{
    public List<RtpcManager> RtpcManagers { get; set; } = new();

    public bool Read(BinaryReader reader)
    {
        var numberOfRtpcs = reader.ReadUInt16();

        for (var i = 0; i < numberOfRtpcs; ++i)
        {
            var rtpcManager = new RtpcManager();

            if (!rtpcManager.Read(reader))
            {
                return false;
            }

            RtpcManagers.Add(rtpcManager);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((ushort) RtpcManagers.Count);

        foreach (var rtpcManager in RtpcManagers)
        {
            rtpcManager.Write(writer);
        }
    }
}
