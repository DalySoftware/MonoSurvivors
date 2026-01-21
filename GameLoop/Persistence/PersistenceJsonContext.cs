using System.Collections.Generic;
using System.Text.Json.Serialization;
using GameLoop.Input;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Persistence;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    Converters =
    [
        typeof(JsonStringEnumConverter<Keys>),
        typeof(JsonStringEnumConverter<Buttons>),
        typeof(KeyBindingsSettingsConverter),
    ])]
[JsonSerializable(typeof(AudioSettings))]
[JsonSerializable(typeof(KeyBindingsSettings))]
[JsonSerializable(typeof(Dictionary<GameplayAction, List<Keys>>))]
[JsonSerializable(typeof(Dictionary<GameplayAction, List<Buttons>>))]
[JsonSerializable(typeof(Dictionary<PauseAction, List<Keys>>))]
[JsonSerializable(typeof(Dictionary<PauseAction, List<Buttons>>))]
[JsonSerializable(typeof(Dictionary<SphereGridAction, List<Keys>>))]
[JsonSerializable(typeof(Dictionary<SphereGridAction, List<Buttons>>))]
[JsonSerializable(typeof(Dictionary<SingleActionSceneAction, List<Keys>>))]
[JsonSerializable(typeof(Dictionary<SingleActionSceneAction, List<Buttons>>))]
public partial class PersistenceJsonContext : JsonSerializerContext;