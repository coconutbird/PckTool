namespace SoundsUnpack.WWise.Structs;

public class FxChunk
{
    public byte FxIndex { get; set; }
    public uint FxId { get; set; }
    public bool IsShareSet { get; set; }
    public bool IsRendered { get; set; }

    public bool Read(BinaryReader reader)
    {
        var fxIndex = reader.ReadByte();
        var fxId = reader.ReadUInt32();
        var isShareSet = reader.ReadByte();
        var isRendered = reader.ReadByte();

        FxIndex = fxIndex;
        FxId = fxId;
        IsShareSet = isShareSet != 0;
        IsRendered = isRendered != 0;

        return true;
    }
}