namespace SoundsUnpack.WWise.Structs;

public class BusInitialParams
{
    public byte BitVector { get; set; }
    public byte BitVector2 { get; set; }
    public ushort MaxNumberOfInstances { get; set; }
    public uint ChannelConfig { get; set; }
    public byte BitVector3 { get; set; }

    public bool Read(BinaryReader reader)
    {
        var bitVector = reader.ReadByte();
        var bitVector2 = reader.ReadByte();

        var maxNumberOfInstances = reader.ReadUInt16();
        var channelConfig = reader.ReadUInt32();

        var bitVector3 = reader.ReadByte();

        BitVector = bitVector;
        BitVector2 = bitVector2;
        MaxNumberOfInstances = maxNumberOfInstances;
        ChannelConfig = channelConfig;
        BitVector3 = bitVector3;

        // TODO: is there anything optional here that needs to be handled?

        return true;
    }
}
