using System.Numerics;

using PckTool.Core.Extensions;
using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

public class RtpcGraphPointBase<TValueType> where TValueType : struct, INumber<TValueType>
{
    public TValueType From { get; set; }
    public TValueType To { get; set; }
    public CurveInterpolation InterpolationType { get; set; }

    public bool Read(BinaryReader reader)
    {
        From = (TValueType) reader.Read(typeof(TValueType));
        To = (TValueType) reader.Read(typeof(TValueType));
        InterpolationType = (CurveInterpolation) reader.ReadUInt32();

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(typeof(TValueType), From);
        writer.Write(typeof(TValueType), To);
        writer.Write((uint) InterpolationType);
    }
}
