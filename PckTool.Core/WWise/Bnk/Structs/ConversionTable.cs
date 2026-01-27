namespace PckTool.Core.WWise.Bnk.Structs;

public class ConversionTable
{
    public byte Scaling { get; set; }
    public List<RtpcGraphPointBase<float>> Points { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        var scaling = reader.ReadByte();
        var points = new List<RtpcGraphPointBase<float>>();

        var numberOfPoints = reader.ReadUInt16();

        for (var i = 0; i < numberOfPoints; ++i)
        {
            var point = new RtpcGraphPointBase<float>();

            if (!point.Read(reader))
            {
                return false;
            }

            points.Add(point);
        }

        Scaling = scaling;
        Points = points;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Scaling);
        writer.Write((ushort) Points.Count);

        foreach (var point in Points)
        {
            point.Write(writer);
        }
    }
}
