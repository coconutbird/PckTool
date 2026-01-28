namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class PropActionSpecificParams
{
    public byte ValueMeaning { get; set; }
    public RandomizerModifier RandomizerModifier { get; set; }

    public bool Read(BinaryReader reader)
    {
        ValueMeaning = reader.ReadByte();

        var randomizerModifier = new RandomizerModifier();

        if (!randomizerModifier.Read(reader))
        {
            return false;
        }

        RandomizerModifier = randomizerModifier;

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(ValueMeaning);
        RandomizerModifier.Write(writer);
    }
}
