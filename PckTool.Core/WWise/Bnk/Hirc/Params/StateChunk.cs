namespace PckTool.Core.WWise.Bnk.Hirc.Params;

public class StateChunk
{
    public List<StateGroup> StateGroups { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        var numberOfStateGroups = reader.ReadUInt32();

        for (var i = 0; i < numberOfStateGroups; i++)
        {
            var stateGroup = new StateGroup();

            if (!stateGroup.Read(reader))
            {
                return false;
            }

            StateGroups.Add(stateGroup);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((uint) StateGroups.Count);

        foreach (var stateGroup in StateGroups)
        {
            stateGroup.Write(writer);
        }
    }
}

public class StateGroup
{
    public uint StateGroupId { get; set; }
    public byte StateSyncType { get; set; }
    public List<StateEntry> States { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        StateGroupId = reader.ReadUInt32();
        StateSyncType = reader.ReadByte();

        var numStates = reader.ReadUInt16();

        for (var i = 0; i < numStates; i++)
        {
            var state = new StateEntry { StateId = reader.ReadUInt32(), StateInstanceId = reader.ReadUInt32() };
            States.Add(state);
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(StateGroupId);
        writer.Write(StateSyncType);
        writer.Write((ushort) States.Count);

        foreach (var state in States)
        {
            writer.Write(state.StateId);
            writer.Write(state.StateInstanceId);
        }
    }
}

public class StateEntry
{
    public uint StateId { get; set; }
    public uint StateInstanceId { get; set; }
}
