using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameLoop.Input.Mapping;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Persistence;

public sealed class KeyBindingsSettingsConverter : JsonConverter<KeyBindingsSettings>
{
    public override KeyBindingsSettings Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Start with defaults
        var result = new KeyBindingsSettings();

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject for KeyBindingsSettings");

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        foreach (var property in root.EnumerateObject())
            switch (property.Name)
            {
                case nameof(KeyBindingsSettings.GameplayActions):
                    MergeKeyMap(result.GameplayActions, property.Value, options);
                    break;

                case nameof(KeyBindingsSettings.PauseMenuActions):
                    MergeKeyMap(result.PauseMenuActions, property.Value, options);
                    break;

                case nameof(KeyBindingsSettings.SphereGridActions):
                    MergeKeyMap(result.SphereGridActions, property.Value, options);
                    break;

                case nameof(KeyBindingsSettings.SingleActionScenes):
                    MergeKeyMap(result.SingleActionScenes, property.Value, options);
                    break;
            }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, KeyBindingsSettings value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(nameof(KeyBindingsSettings.GameplayActions));
        JsonSerializer.Serialize(writer, value.GameplayActions, options);

        writer.WritePropertyName(nameof(KeyBindingsSettings.PauseMenuActions));
        JsonSerializer.Serialize(writer, value.PauseMenuActions, options);

        writer.WritePropertyName(nameof(KeyBindingsSettings.SphereGridActions));
        JsonSerializer.Serialize(writer, value.SphereGridActions, options);

        writer.WritePropertyName(nameof(KeyBindingsSettings.SingleActionScenes));
        JsonSerializer.Serialize(writer, value.SingleActionScenes, options);

        writer.WriteEndObject();
    }

    private static void MergeKeyMap<TAction>(ActionKeyMap<TAction> target, JsonElement jsonElement,
        JsonSerializerOptions options)
        where TAction : struct, Enum
    {
        if (jsonElement.TryGetProperty(nameof(ActionKeyMap<TAction>.Keyboard), out var keyboardProp))
        {
            var keyboard =
                JsonSerializer.Deserialize<Dictionary<TAction, List<Keys>>>(keyboardProp.GetRawText(), options);
            if (keyboard != null)
                target.MergeFrom(new ActionKeyMap<TAction> { Keyboard = keyboard });
        }

        if (jsonElement.TryGetProperty(nameof(ActionKeyMap<TAction>.Gamepad), out var gamepadProp))
        {
            var gamepad =
                JsonSerializer.Deserialize<Dictionary<TAction, List<Buttons>>>(gamepadProp.GetRawText(), options);
            if (gamepad != null)
                target.MergeFrom(new ActionKeyMap<TAction> { Gamepad = gamepad });
        }
    }
}