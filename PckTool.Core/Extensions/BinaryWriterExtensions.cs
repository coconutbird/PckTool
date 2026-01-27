namespace PckTool.Core.Extensions;

public static class BinaryWriterExtensions
{
    /// <summary>
    ///     Writes a value of the specified type to the binary writer.
    /// </summary>
    /// <param name="writer">The binary writer.</param>
    /// <param name="type">The type to write.</param>
    /// <param name="value">The value to write.</param>
    public static void Write(this BinaryWriter writer, Type type, object value)
    {
        switch (value)
        {
            case float f when type == typeof(float):
                writer.Write(f);

                break;

            case double d when type == typeof(double):
                writer.Write(d);

                break;

            case byte b when type == typeof(byte):
                writer.Write(b);

                break;

            case sbyte sb when type == typeof(sbyte):
                writer.Write(sb);

                break;

            case short s when type == typeof(short):
                writer.Write(s);

                break;

            case ushort us when type == typeof(ushort):
                writer.Write(us);

                break;

            case int i when type == typeof(int):
                writer.Write(i);

                break;

            case uint u when type == typeof(uint):
                writer.Write(u);

                break;

            case long l when type == typeof(long):
                writer.Write(l);

                break;

            case ulong ul when type == typeof(ulong):
                writer.Write(ul);

                break;

            default:
                throw new NotSupportedException($"Type {type.Name} is not supported for binary serialization");
        }
    }
}
