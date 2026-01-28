namespace PckTool.Core.WWise.Bnk.Hirc.Items;

/// <summary>
///     State initial values for bank version 113.
///     Corresponds to CAkState::SetInitialValues in wwiser.
///     For v57-126, uses AkPropBundle&lt;float&gt; format.
/// </summary>
public class StateInitialValues
{
    /// <summary>
    ///     Property bundle containing state property values.
    ///     Each property has an ID (RTPC parameter ID) and a float value.
    /// </summary>
    public List<StateProp> Props { get; set; } = [];

    public bool Read(BinaryReader reader)
    {
        // For v57-126: AkPropBundle<float>::SetInitialParams
        // cProps (u8)
        var numProps = reader.ReadByte();

        // First read all property IDs
        var propIds = new byte[numProps];

        for (var i = 0; i < numProps; i++)
        {
            propIds[i] = reader.ReadByte();
        }

        // Then read all property values
        for (var i = 0; i < numProps; i++)
        {
            Props.Add(new StateProp { PropId = propIds[i], Value = reader.ReadSingle() });
        }

        return true;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Props.Count);

        // Write all IDs first
        foreach (var prop in Props)
        {
            writer.Write(prop.PropId);
        }

        // Then write all values
        foreach (var prop in Props)
        {
            writer.Write(prop.Value);
        }
    }
}

/// <summary>
///     A state property with ID and value.
/// </summary>
public class StateProp
{
    /// <summary>
    ///     Property ID (RTPC parameter ID).
    /// </summary>
    public byte PropId { get; set; }

    /// <summary>
    ///     Property value.
    /// </summary>
    public float Value { get; set; }
}
