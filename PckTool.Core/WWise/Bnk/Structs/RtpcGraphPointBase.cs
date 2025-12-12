using System.Numerics;

using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

public class RtpcGraphPointBase<TValueType> where TValueType : struct, INumber<TValueType>
{
    public TValueType From { get; set; }
    public TValueType To { get; set; }
    public CurveInterpolation InterpolationType { get; set; }

    public bool Read(BinaryReader reader)
    {
        From = ReadValue(reader);
        To = ReadValue(reader);
        InterpolationType = (CurveInterpolation) reader.ReadUInt32();

        return true;
    }

    private static TValueType ReadValue(BinaryReader reader)
    {
        return typeof(TValueType) switch
        {
            var t when t == typeof(float) => (TValueType) (object) reader.ReadSingle(),
            var t when t == typeof(uint) => (TValueType) (object) reader.ReadUInt32(),
            var t when t == typeof(int) => (TValueType) (object) reader.ReadInt32(),
            var t when t == typeof(double) => (TValueType) (object) reader.ReadDouble(),
            var t when t == typeof(short) => (TValueType) (object) reader.ReadInt16(),
            var t when t == typeof(ushort) => (TValueType) (object) reader.ReadUInt16(),
            var t when t == typeof(byte) => (TValueType) (object) reader.ReadByte(),
            var t when t == typeof(sbyte) => (TValueType) (object) reader.ReadSByte(),
            var t when t == typeof(long) => (TValueType) (object) reader.ReadInt64(),
            var t when t == typeof(ulong) => (TValueType) (object) reader.ReadUInt64(),
            _ => throw new NotSupportedException(
                $"Type {typeof(TValueType).Name} is not supported for binary deserialization")
        };
    }
}
