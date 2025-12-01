namespace SoundsUnpack.WWise.Structs;

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
}