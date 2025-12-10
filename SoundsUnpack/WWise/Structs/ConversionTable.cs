using SoundsUnpack.WWise.Bank;
using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Structs;

public class ConversionTable
{
    public byte Scaling { get; set; }
    public List<RtpcGraphPointBase<float>> Points { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        var scaling = reader.ReadByte();
        var points = new List<RtpcGraphPointBase<float>>();

        var numberOfRtpcs = reader.ReadUInt16();

        for (var i = 0; i < numberOfRtpcs; ++i)
        {
            var from = reader.ReadSingle();
            var to = reader.ReadSingle();
            var interpolationType = (CurveInterpolation) reader.ReadUInt32();

            points.Add(new RtpcGraphPointBase<float> { From = from, To = to, InterpolationType = interpolationType });
        }

        Scaling = scaling;
        Points = points;

        return true;
    }
}
