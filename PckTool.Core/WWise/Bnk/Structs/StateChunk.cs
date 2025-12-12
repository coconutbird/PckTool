namespace PckTool.Core.WWise.Bnk.Structs;

public class StateChunk
{
    public bool Read(BinaryReader reader)
    {
        var numberOfStateGroups = reader.ReadUInt32();

        if (numberOfStateGroups > 0)
        {
            throw new NotImplementedException("StateChunk with StateGroups is not implemented.");
        }

        return true;
    }
}
