using System.Text;

using PckTool.Core.WWise.Bnk.Enums;
using PckTool.Core.WWise.Util;

namespace PckTool.Core.WWise.Bnk.Chunks;

/// <summary>
///     Represents the STID (String Map) chunk of a soundbank.
///     Contains string mappings for bank IDs to names.
/// </summary>
public class StringMapChunk : BaseChunk
{
    public override bool IsValid => true;

    public override uint Magic => Hash.AkmmioFourcc('S', 'T', 'I', 'D');

    /// <summary>
    ///     The string type (typically Bank = 1).
    /// </summary>
    public BnkStringType StringType { get; set; }

    /// <summary>
    ///     Map of bank IDs to their string names.
    /// </summary>
    public Dictionary<uint, string> BankNames { get; } = new();

    protected override bool ReadInternal(SoundBank soundBank, BinaryReader reader, uint size, long startPosition)
    {
        StringType = (BnkStringType) reader.ReadUInt32();

        if (StringType == BnkStringType.Bank)
        {
            var numberOfStrings = reader.ReadUInt32();

            for (var i = 0; i < numberOfStrings; ++i)
            {
                var bankId = reader.ReadUInt32();
                var stringSize = reader.ReadByte();
                var stringBuffer = reader.ReadBytes(stringSize);
                var str = Encoding.ASCII.GetString(stringBuffer);

                BankNames[bankId] = str;
            }
        }

        // Ensure we're at the end of the chunk
        reader.BaseStream.Position = startPosition + size;

        return true;
    }

    protected override void WriteInternal(SoundBank soundBank, BinaryWriter writer)
    {
        writer.Write((uint) StringType);

        if (StringType == BnkStringType.Bank)
        {
            writer.Write((uint) BankNames.Count);

            foreach (var (bankId, name) in BankNames)
            {
                writer.Write(bankId);
                var bytes = Encoding.ASCII.GetBytes(name);
                writer.Write((byte) bytes.Length);
                writer.Write(bytes);
            }
        }
    }
}

