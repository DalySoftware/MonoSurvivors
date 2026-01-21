using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GameLoop.Persistence;

namespace Veil.Desktop.PlatformServices.Persistence;

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