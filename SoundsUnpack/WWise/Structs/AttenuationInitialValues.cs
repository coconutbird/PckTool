namespace SoundsUnpack.WWise.Structs;

public class AttenuationInitialValues
{
    public bool IsConeEnabled { get; set; }
    public sbyte[] CurveToUse { get; set; } = new sbyte[6];
    public List<ConversionTable> Curves { get; set; } = [];
    public InitialRtpc InitialRtpc { get; set; }

    public bool Read(BinaryReader reader)
    {
        IsConeEnabled = reader.ReadByte() == 1;

        CurveToUse = new sbyte[7];
        for (var i = 0; i < 7; i++)
        {
            CurveToUse[i] = reader.ReadSByte();
        }

        var numberOfCurves = reader.ReadByte();

        var curves = new List<ConversionTable>();
        for (var i = 0; i < numberOfCurves; ++i)
        {
            var curve = new ConversionTable();

            if (!curve.Read(reader))
            {
                return false;
            }

            curves.Add(curve);
        }

        var initialRtpc = new InitialRtpc();
        if (!initialRtpc.Read(reader))
        {
            return false;
        }

        Curves = curves;
        InitialRtpc = initialRtpc;

        return true;
    }
}