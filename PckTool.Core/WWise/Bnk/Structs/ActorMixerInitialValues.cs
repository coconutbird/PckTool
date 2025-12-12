namespace PckTool.Core.WWise.Bnk.Structs;

public class ActorMixerInitialValues
{
    public NodeBaseParams NodeBaseParams { get; set; }
    public Children Children { get; set; }

    public bool Read(BinaryReader reader)
    {
        var nodeBaseParams = new NodeBaseParams();

        if (!nodeBaseParams.Read(reader))
        {
            return false;
        }

        var children = new Children();

        if (!children.Read(reader))
        {
            return false;
        }

        NodeBaseParams = nodeBaseParams;
        Children = children;

        return true;
    }
}
