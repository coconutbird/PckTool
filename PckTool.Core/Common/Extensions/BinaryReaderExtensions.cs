using System.Text;

namespace PckTool.Core.Common.Extensions;

public static class BinaryReaderExtensions
{
    public static string ReadWString(this BinaryReader reader)
    {
        var builder = new StringBuilder();

        while (true)
        {
            var buffer = reader.ReadUInt16();

            if (buffer == 0)
            {
                return builder.ToString();
            }

            builder.Append((char) buffer);
        }
    }

    /// <summary>
    ///     Reads a value of the specified type from the binary reader.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <param name="type">The type to read.</param>
    /// <returns>The value read, boxed as object.</returns>
    public static object Read(this BinaryReader reader, Type type)
    {
        return type switch
        {
            _ when type == typeof(float) => reader.ReadSingle(),
            _ when type == typeof(double) => reader.ReadDouble(),
            _ when type == typeof(byte) => reader.ReadByte(),
            _ when type == typeof(sbyte) => reader.ReadSByte(),
            _ when type == typeof(short) => reader.ReadInt16(),
            _ when type == typeof(ushort) => reader.ReadUInt16(),
            _ when type == typeof(int) => reader.ReadInt32(),
            _ when type == typeof(uint) => reader.ReadUInt32(),
            _ when type == typeof(long) => reader.ReadInt64(),
            _ when type == typeof(ulong) => reader.ReadUInt64(),
            _ => throw new NotSupportedException($"Type {type.Name} is not supported for binary deserialization")
        };
    }
}
