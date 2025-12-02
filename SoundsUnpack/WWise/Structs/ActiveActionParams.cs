using SoundsUnpack.WWise.Enums;
using SoundsUnpack.WWise.Helpers;

namespace SoundsUnpack.WWise.Structs;

public class ActiveActionParams
{
    public byte BitVector { get; set; }

    public byte FadeCurve
    {
        get => (byte)(BitVector & 0x0F);
        set => BitVector = (byte)((BitVector & 0xF0) | (value & 0x0F));
    }

    public ResumeActionSpecificParams? ResumeActionSpecificParams { get; set; }

    public ExceptParams ExceptParams { get; set; }

    public bool Read(BinaryReader reader, ActionType actionType)
    {
        var bitVector = reader.ReadByte();

        ResumeActionSpecificParams? resumeActionSpecificParams = null;
        if (ActionTypeHelpers.IsResumeActionType(actionType))
        {
            resumeActionSpecificParams = new ResumeActionSpecificParams();
            if (!resumeActionSpecificParams.Read(reader))
            {
                return false;
            }

            ResumeActionSpecificParams = resumeActionSpecificParams;
        }

        var exceptParams = new ExceptParams();
        if (!exceptParams.Read(reader))
        {
            return false;
        }

        BitVector = bitVector;
        ResumeActionSpecificParams = resumeActionSpecificParams;
        ExceptParams = exceptParams;

        return true;
    }
}