using SoundsUnpack.WWise.Enums;

namespace SoundsUnpack.WWise.Bank;

public class RtpcGraphPointBase<TValueType> where TValueType : struct
{
    public TValueType From { get; set; }
    public TValueType To { get; set; }
    public CurveInterpolation InterpolationType { get; set; }
}