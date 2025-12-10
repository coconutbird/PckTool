namespace SoundsUnpack.WWise.Structs;

/// <summary>
///     Bus FX parameters for bank version 113 (v90-135 range in wwiser).
///     Corresponds to CAkBus::SetInitialFxParams in wwiser.
/// </summary>
public class BusInitialFxParams
{
  public byte NumFx { get; set; }
  public byte BitsFxBypass { get; set; }
  public List<FxChunk> FxChunks { get; set; } = [];
  public uint FxId0 { get; set; }
  public byte IsShareSet0 { get; set; }

  public bool Read(BinaryReader reader)
  {
    // v113 falls in the 90-135 range in wwiser
    // See CAkBus__SetInitialFxParams in wparser.py

    var numFx = reader.ReadByte();
    byte bitsFxBypass = 0;
    var fxChunks = new List<FxChunk>();

    if (numFx > 0)
    {
      bitsFxBypass = reader.ReadByte();

      for (var i = 0; i < numFx; i++)
      {
        var fxChunk = new FxChunk();

        if (!fxChunk.Read(reader))
        {
          return false;
        }

        fxChunks.Add(fxChunk);
      }
    }

    // For v90-145: fxID_0 and bIsShareSet_0 are always read at the end
    var fxId0 = reader.ReadUInt32();
    var isShareSet0 = reader.ReadByte();

    NumFx = numFx;
    BitsFxBypass = bitsFxBypass;
    FxChunks = fxChunks;
    FxId0 = fxId0;
    IsShareSet0 = isShareSet0;

    return true;
  }
}
