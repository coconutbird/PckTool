namespace SoundsUnpack.WWise.Structs;

public class InitialRtpc
{
    public bool Read(BinaryReader reader)
    {
        var numberOfRtpcs = reader.ReadUInt16();
        if (numberOfRtpcs == 0)
        {
            return true;
        }

        return false;
    }
}