using System.Text;

namespace PckTool.Core.Extensions;

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
}
