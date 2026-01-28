using System.Numerics;

using PckTool.Core.Common.Extensions;
using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class RtpcGraphPointBase<TValueType> where TValueType : struct, INumber<TValueType>
{
    public TValueType From { get; set; }
    public TValueType To { get; set; }
    public CurveInterpolation InterpolationType { get; set; }

    public bool Read(BinaryReader reader)
    {
        From = (TValueType) Convert.ChangeType(reader.Read(typeof(TValueType)), typeof(TValueType));
        To = (TValueType) Convert.ChangeType(reader.Read(typeof(TValueType)), typeof(TValueType));
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
