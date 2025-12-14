using System;
using System.Collections.Generic;
using System.Text.Json;
using GameLoop.Persistence;
using Microsoft.Extensions.Configuration;

namespace GameLoop.UserSettings;

public sealed class PersistenceConfigurationProvider : ConfigurationProvider, IDisposable
{
    private readonly IPersistence _persistence;
    private readonly string _storageKey;

    public PersistenceConfigurationProvider(IPersistence persistence, string storageKey)
    {
        _persistence = persistence;
        _storageKey = storageKey;

        _persistence.OnChanged += OnPersistenceChanged;
    }

    public void Dispose() => _persistence.OnChanged -= OnPersistenceChanged;

    public override void Load()
    {
        try
        {
            var data = _persistence.Load<GameSettings>(_storageKey);
            Data = FlattenObject(data);
        }
        catch
        {
            Data = FlattenObject(new GameSettings());
        }
    }

    public override void Set(string key, string? value)
    {
        base.Set(key, value);
        Save();
    }

    private void OnPersistenceChanged(string storageKey)
    {
        if (!string.Equals(storageKey, _storageKey, StringComparison.OrdinalIgnoreCase)) return;
        Load();
        OnReload();
    }

    private void Save()
    {
        var obj = UnflattenObject(Data);
        _persistence.Save(obj, _storageKey);
    }

    private static GameSettings UnflattenObject(IDictionary<string, string?> data)
    {
        var jsonDict = new Dictionary<string, object?>();

        foreach (var kvp in data)
        {
            SetNestedValue(jsonDict, kvp.Key, kvp.Value);
        }

        var json = JsonSerializer.Serialize(jsonDict);
        return JsonSerializer.Deserialize<GameSettings>(json) ?? new GameSettings();
    }

    private static void SetNestedValue(Dictionary<string, object?> dict, string key, string? value)
    {
        var parts = key.Split(':');
        var current = dict;

        foreach (var part in parts[..^1])
        {
            if (!current.TryGetValue(part, out var next))
                current[part] = next = new Dictionary<string, object?>();
            current = (Dictionary<string, object?>)next!;
        }

        current[parts[^1]] = value;
    }

    private static Dictionary<string, string?> FlattenObject(GameSettings obj)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var json = JsonSerializer.Serialize(obj);
        using var doc = JsonDocument.Parse(json);

        void Flatten(JsonElement element, string prefix)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                        Flatten(prop.Value, string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}:{prop.Name}");
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                        Flatten(item, $"{prefix}:{index++}");
                    break;

                default:
                    result[prefix] = element.ToString();
                    break;
            }
        }

        Flatten(doc.RootElement, "");
        return result;
    }
}