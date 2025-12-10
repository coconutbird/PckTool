using SoundsUnpack.WWise.Structs;

namespace SoundsUnpack.WWise.Bank;

/// <summary>
///     Environment settings conversion table for bank version 113.
///     Corresponds to CAkBankMgr::ProcessEnvSettingsChunk in wwiser.
///     Contains a 2D array of ObsOccCurves.
///     For v90-150: max_x=2 (Obstruction, Occlusion), max_y=3 (Volume, LPF, HPF) = 6 curves
/// </summary>
public class ConversionTable
{
  public bool IsValid => true;

  /// <summary>
  ///     2D array of curves indexed by [CurveXType][CurveYType]
  ///     For v113: [0-1][0-2] = 6 curves total
  /// </summary>
  public List<ObsOccCurve> Curves { get; set; } = [];

  public bool Read(BinaryReader reader, uint size)
  {
    // For v113 (v90-150): max_x=2, max_y=3
    const int maxX = 2;
    const int maxY = 3;

    var curves = new List<ObsOccCurve>();

    for (var x = 0; x < maxX; x++)
    {
      for (var y = 0; y < maxY; y++)
      {
        var curve = new ObsOccCurve { XType = x, YType = y };

        if (!curve.Read(reader))
        {
          return false;
        }

        curves.Add(curve);
      }
    }

    Curves = curves;

    return true;
  }
}

/// <summary>
///     Obstruction/Occlusion curve entry.
///     Maps to ObsOccCurve[eCurveXType][eCurveYType] in wwiser.
/// </summary>
public class ObsOccCurve
{
  /// <summary>
  ///     X-axis type: 0=Obstruction, 1=Occlusion
  /// </summary>
  public int XType { get; set; }

  /// <summary>
  ///     Y-axis type: 0=Volume, 1=LPF, 2=HPF
  /// </summary>
  public int YType { get; set; }

  public bool CurveEnabled { get; set; }
  public byte CurveScaling { get; set; }
  public List<RtpcGraphPointBase<float>> Points { get; set; } = [];

  public bool Read(BinaryReader reader)
  {
    // For v>36: u8 enabled, u8 scaling, u16 size
    CurveEnabled = reader.ReadByte() != 0;
    CurveScaling = reader.ReadByte();

    var numberOfPoints = reader.ReadUInt16();
    var points = new List<RtpcGraphPointBase<float>>();

    for (var i = 0; i < numberOfPoints; i++)
    {
      var point = new RtpcGraphPointBase<float>();

      if (!point.Read(reader))
      {
        return false;
      }

      points.Add(point);
    }

    Points = points;

    return true;
  }
}
