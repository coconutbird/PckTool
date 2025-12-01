namespace SoundsUnpack.WWise.Structs;

public class AttenuationInitialValues
{
    public bool IsConeEnabled { get; set; }
    public sbyte[] CurveToUse { get; set; } = new sbyte[6];
    public List<ConversionTable> Curves { get; set; } = [];
    public object InitialRtpc { get; set; } // TODO: Define proper type
    
    public bool Read(BinaryReader reader)
    {
        IsConeEnabled = reader.ReadBoolean();

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

        // InitialRtpc reading logic would go here
        var numberOfRtpcs = reader.ReadUInt16();
        if (numberOfRtpcs > 0)
        {
            return false; // Not implemented
        }
        
        Curves = curves;

        return true;
    }
}