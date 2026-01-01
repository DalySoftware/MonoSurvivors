using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using GameLoop.UserSettings;

namespace GameLoop.Persistence;

public sealed class AppDataPersistence : IPersistence
{
    private static string SettingsFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MonoSurvivors"
        );

    public event Action<string>? OnChanged;

    public T Load<T>(string storageKey, JsonTypeInfo<T> typeInfo)
    {
        var path = Path.Combine(SettingsFolder, SanitizeStorageKey(storageKey));
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize(json, typeInfo) ??
               throw new InvalidOperationException($"Failed to deserialize {storageKey}");
    }

    public void Save<T>(T data, string storageKey, JsonTypeInfo<T> typeInfo)
    {
        Directory.CreateDirectory(SettingsFolder);

        var fileName = SanitizeStorageKey(storageKey);
        var json = JsonSerializer.Serialize(data, typeInfo);
        var path = Path.Combine(SettingsFolder, fileName);
        File.WriteAllText(path, json);

        OnChanged?.Invoke(storageKey);
    }

    private static string SanitizeStorageKey(string storageKey) =>
        storageKey.Replace(':', '_') + ".json";
}

[JsonSerializable(typeof(GameSettings))]
[JsonSerializable(typeof(PersistedConfiguration))]
public partial class PersistenceJsonContext : JsonSerializerContext;

public sealed class PersistedConfiguration
{
    public Dictionary<string, string?> Values { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}