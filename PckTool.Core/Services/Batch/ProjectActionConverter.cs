using System.Text.Json;
using System.Text.Json.Serialization;

using PckTool.Abstractions.Batch;

namespace PckTool.Core.Services.Batch;

/// <summary>
///     Custom JSON converter for polymorphic IProjectAction serialization/deserialization.
/// </summary>
public sealed class ProjectActionConverter : JsonConverter<IProjectAction>
{
    /// <inheritdoc />
    public override IProjectAction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("action", out var actionProp))
        {
            throw new JsonException("Action object must have an 'action' property.");
        }

        var actionType = actionProp.Deserialize<ProjectActionType>(options);

        var json = root.GetRawText();

        return actionType switch
        {
            ProjectActionType.Replace => JsonSerializer.Deserialize<ReplaceAction>(json, options),
            ProjectActionType.Add => JsonSerializer.Deserialize<AddAction>(json, options),
            ProjectActionType.Remove => JsonSerializer.Deserialize<RemoveAction>(json, options),
            _ => throw new JsonException($"Unknown action type: {actionType}")
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, IProjectAction value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ReplaceAction replaceAction:
                JsonSerializer.Serialize(writer, replaceAction, options);

                break;
            case AddAction addAction:
                JsonSerializer.Serialize(writer, addAction, options);

                break;
            case RemoveAction removeAction:
                JsonSerializer.Serialize(writer, removeAction, options);

                break;
            default:
                throw new JsonException($"Unknown action type: {value.GetType().Name}");
        }
    }
}
