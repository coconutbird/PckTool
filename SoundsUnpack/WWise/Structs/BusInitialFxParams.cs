namespace SoundsUnpack.WWise.Structs;

public class BusInitialFxParams
{
    public byte BitsFxBypass { get; set; }
    public List<FxChunk> FxChunks { get; set; } = [];
    public uint FxId0 { get; set; }
    public bool IsShareSet { get; set; }

    public bool Read(BinaryReader reader)
    {
        // TODO: actually implement this
        
        return false;
        
        var numberOfFx = reader.ReadByte();
        var bitFxBypass = reader.ReadByte() != 0;
        if (bitFxBypass)
        {
            throw new NotImplementedException("BitsFxBypass with bypass");
        }

        var fxChunks = new List<FxChunk>();
        for (var i = 0; i < numberOfFx; i++)
        {
            var fxChunk = new FxChunk();
            if (!fxChunk.Read(reader))
            {
                return false;
            }

            fxChunks.Add(fxChunk);
        }

        var fxId0 = reader.ReadUInt32();
        var isShareSet = reader.ReadByte() != 0;

        BitsFxBypass = bitFxBypass ? (byte)1 : (byte)0;
        FxChunks = fxChunks;
        FxId0 = fxId0;
        IsShareSet = isShareSet;

        return true;
    }
}