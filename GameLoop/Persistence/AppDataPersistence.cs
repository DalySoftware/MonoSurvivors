using System;
using System.IO;
using System.Text.Json;

namespace GameLoop.Persistence;

public sealed class AppDataPersistence : IPersistence
{
    private static string SettingsFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MonoSurvivors"
        );

    public event Action<string>? OnChanged;

    public T Load<T>(string storageKey)
    {
        var fileName = SanitizeStorageKey(storageKey);
        var path = Path.Combine(SettingsFolder, fileName);
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException($"Failed to deserialize {storageKey}");
    }

    public void Save<T>(T data, string storageKey)
    {
        Directory.CreateDirectory(SettingsFolder);

        var fileName = SanitizeStorageKey(storageKey);
        var json = JsonSerializer.Serialize(data);
        var path = Path.Combine(SettingsFolder, fileName);
        File.WriteAllText(path, json);

        OnChanged?.Invoke(storageKey);
    }

    private static string SanitizeStorageKey(string storageKey) =>
        storageKey.Replace(':', '_') + ".json";
}