namespace SoundsUnpack.WWise.Structs;

public class ActionPlay
{
    public ushort ActionType { get; set; }
    public ActionInitialValues ActionInitialValues { get; set; }

    public bool Read(BinaryReader reader)
    {
        var actionType = reader.ReadUInt16();
        var actionInitialValues = new ActionInitialValues();
        if (!actionInitialValues.Read(reader, actionType))
        {
            return false;
        }

        ActionInitialValues = actionInitialValues;

        return true;
    }
}