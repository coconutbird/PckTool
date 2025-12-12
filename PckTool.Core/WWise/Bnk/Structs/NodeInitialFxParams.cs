namespace PckTool.Core.WWise.Bnk.Structs;

public class NodeInitialFxParams
{
    public byte IsOverrideParentFx { get; set; }
    public byte NumFx { get; set; }
    public bool FxBypass { get; set; }
    public List<FxChunk> FxChunks { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        FxChunks.Clear();

        IsOverrideParentFx = reader.ReadByte();
        NumFx = reader.ReadByte();

        if (NumFx > 0)
        {
            var bitsFxBypass = reader.ReadByte();
            FxBypass = (bitsFxBypass & 0x01) != 0;

            for (var i = 0; i < NumFx; ++i)
            {
                var chunk = new FxChunk();

                if (!chunk.Read(reader))
                {
                    return false;
                }

                FxChunks.Add(chunk);
            }
        }

        return true;
    }
}
