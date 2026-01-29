namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class RtpcManager
{
    public uint RptcId { get; set; }
    public byte RtpcType { get; set; }
    public byte RtpcAccum { get; set; }
    public byte ParamId { get; set; }
    public uint RtpcCurveId { get; set; }
    public byte Scaling { get; set; }
    public List<RtpcGraphPointBase<float>> GraphPoints { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        var rptcId = reader.ReadUInt32();
        var rtpcType = reader.ReadByte();
        var rtpcAccum = reader.ReadByte();
        var paramId = reader.ReadByte();
        var rtpcCurveId = reader.ReadUInt32();
        var scaling = reader.ReadByte();
        var numberOfGraphPoints = reader.ReadUInt16();
        var graphPoints = new List<RtpcGraphPointBase<float>>();

        for (var i = 0; i < numberOfGraphPoints; ++i)
        {
            var graphPoint = new RtpcGraphPointBase<float>();

            if (!graphPoint.Read(reader))
            {
                return false;
            }

            graphPoints.Add(graphPoint);
        }

        RptcId = rptcId;
        RtpcType = rtpcType;
        RtpcAccum = rtpcAccum;
        ParamId = paramId;
        RtpcCurveId = rtpcCurveId;
        Scaling = scaling;
        GraphPoints = graphPoints;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(RptcId);
        writer.Write(RtpcType);
        writer.Write(RtpcAccum);
        writer.Write(ParamId);
        writer.Write(RtpcCurveId);
        writer.Write(Scaling);
        writer.Write((ushort) GraphPoints.Count);

        foreach (var graphPoint in GraphPoints)
        {
            graphPoint.Write(writer);
        }
    }
}
