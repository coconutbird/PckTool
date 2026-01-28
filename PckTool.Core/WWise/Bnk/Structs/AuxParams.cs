using PckTool.Core.WWise.Bnk.Enums;

namespace PckTool.Core.WWise.Bnk.Structs;

public class AuxParams
{
    public AuxFlags Flags { get; set; }

    public bool OverrideUserAuxSends
    {
        get => Flags.HasFlag(AuxFlags.OverrideUserAuxSends);
        set => Flags = value ? Flags | AuxFlags.OverrideUserAuxSends : Flags & ~AuxFlags.OverrideUserAuxSends;
    }

    public bool HasAux
    {
        get => Flags.HasFlag(AuxFlags.HasAux);
        set => Flags = value ? Flags | AuxFlags.HasAux : Flags & ~AuxFlags.HasAux;
    }

    public bool OverrideReflectionsAuxBus
    {
        get => Flags.HasFlag(AuxFlags.OverrideReflectionsAuxBus);
        set =>
            Flags = value
                ? Flags | AuxFlags.OverrideReflectionsAuxBus
                : Flags & ~AuxFlags.OverrideReflectionsAuxBus;
    }

    public uint[]? AuxIds { get; set; }

    public bool Read(BinaryReader reader)
    {
        Flags = (AuxFlags) reader.ReadByte();

        if (HasAux)
        {
            var auxIds = new uint[4];

            for (var i = 0; i < auxIds.Length; i++)
            {
                auxIds[i] = reader.ReadUInt32();
            }

            AuxIds = auxIds;
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Flags);

        if (HasAux && AuxIds != null)
        {
            foreach (var auxId in AuxIds)
            {
                writer.Write(auxId);
            }
        }
    }
}
