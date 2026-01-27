namespace PckTool.Core.WWise.Bnk.Structs;

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
        var isShareSet = reader.ReadByte() != 0;
        var isRendered = reader.ReadByte() != 0;

        FxIndex = fxIndex;
        FxId = fxId;
        IsShareSet = isShareSet;
        IsRendered = isRendered;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(FxIndex);
        writer.Write(FxId);
        writer.Write((byte) (IsShareSet ? 1 : 0));
        writer.Write((byte) (IsRendered ? 1 : 0));
    }
}
