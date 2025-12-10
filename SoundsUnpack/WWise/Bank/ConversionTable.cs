using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Bank;

public class ConversionTable
{
    public bool IsValid => true;

    public bool CurveEnabled { get; set; }
    public byte CurveScaling { get; set; }
    public List<RtpcGraphPointBase<float>> Points { get; set; } = new();

    public bool Read(BinaryReader reader, uint size)
    {
        Points.Clear();

        CurveEnabled = reader.ReadByte() != 0;
        CurveScaling = reader.ReadByte();

        var numberOfPoints = reader.ReadUInt16();

        for (var i = 0; i < numberOfPoints; ++i)
        {
            var point = new RtpcGraphPointBase<float>();

            if (!point.Read(reader))
            {
                return false;
            }

            Points.Add(point);
        }

        return true;
    }
}
