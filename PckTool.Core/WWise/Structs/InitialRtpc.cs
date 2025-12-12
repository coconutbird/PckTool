namespace PckTool.Core.WWise.Structs;

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
}
