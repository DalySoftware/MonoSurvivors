using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GameLoop.Input;
using GameLoop.UserSettings;
using Microsoft.Xna.Framework.Input;

namespace GameLoop.Persistence;

public sealed class AppDataPersistence : ISettingsPersistence
{
    private static string SettingsFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MonoSurvivors"
        );

    public void Save<T>(T data, JsonTypeInfo<T> typeInfo)
    {
        Directory.CreateDirectory(SettingsFolder);

        var storageKey = GetStorageKey<T>();
        var fileName = SanitizeStorageKey(storageKey);
        var path = Path.Combine(SettingsFolder, fileName);

        var json = JsonSerializer.Serialize(data, typeInfo);
        File.WriteAllText(path, json);

        OnChanged?.Invoke(typeof(T)); // <-- now type-based
    }

    public T Load<T>(JsonTypeInfo<T> typeInfo) where T : new()
    {
        var storageKey = GetStorageKey<T>();
        var path = Path.Combine(SettingsFolder, SanitizeStorageKey(storageKey));

        if (!File.Exists(path))
            return new T();

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize(json, typeInfo) ?? new T();
    }

    public event Action<Type>? OnChanged;

    private static string GetStorageKey<T>() => (typeof(T).FullName ?? typeof(T).Name) + ".json";

    private static string SanitizeStorageKey(string key) =>
        Path.GetInvalidFileNameChars().Aggregate(key, (current, c) => current.Replace(c, '_'));
}

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