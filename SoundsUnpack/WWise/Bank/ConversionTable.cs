using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Bank;

public class ConversionTable
{
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
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var interpolation = (CurveInterpolation)reader.ReadUInt32();

            Points.Add(new RtpcGraphPointBase<float>
            {
                From = x,
                To = y,
                InterpolationType = interpolation
            });
        }

        return true;
    }
}