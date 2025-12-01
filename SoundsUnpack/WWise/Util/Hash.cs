using System.Text;

namespace SoundsUnpack.WWise;

public static class Hash
{
    public static uint GetIdFromString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        // Remove the file extension if present
        var dotIndex = input.LastIndexOf('.');
        if (dotIndex > 0)
        {
            input = input.Substring(0, dotIndex);
        }

        return Fnv132(input);
    }

    // public static uint Fnv1AHaloWars32(string input)
    // {
    //     if (string.IsNullOrEmpty(input))
    //         return 0;
    // 
    //     var bytes = Encoding.UTF8.GetBytes(input.ToLowerInvariant());
    // 
    //     uint hash = 0x811C9DC5;
    //     foreach (byte b in bytes)
    //     {
    //         hash = b ^ 0x1000193 * hash;
    //     }
    // 
    //     return hash;
    // }

    public static uint Fnv132(string input)
    {
        if (string.IsNullOrEmpty(input))
            return 0;

        var bytes = Encoding.UTF8.GetBytes(input.ToLowerInvariant());

        uint hash = 0x811C9DC5;
        foreach (byte b in bytes)
        {
            hash *= 0x1000193;
            hash ^= b;
        }

        return hash;
    }

    public static uint AkmmioFourcc(char ch0, char ch1, char ch2, char ch3)
    {
        return (byte)ch0
               | ((uint)(byte)ch1 << 8)
               | ((uint)(byte)ch2 << 16)
               | ((uint)(byte)ch3 << 24);
    }
}