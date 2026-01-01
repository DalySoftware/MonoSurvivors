using System;
using System.Collections.Generic;
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
            var persisted = _persistence.Load(_storageKey, PersistenceJsonContext.Default.PersistedConfiguration);

            Data = new Dictionary<string, string?>(persisted.Values, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public override void Set(string key, string? value)
    {
        base.Set(key, value);
        Save();
    }

    private void Save()
    {
        var persisted = new PersistedConfiguration
        {
            Values = new Dictionary<string, string?>(Data, StringComparer.OrdinalIgnoreCase),
        };

        _persistence.Save(persisted, _storageKey, PersistenceJsonContext.Default.PersistedConfiguration);
    }

    private void OnPersistenceChanged(string storageKey)
    {
        if (!string.Equals(storageKey, _storageKey, StringComparison.OrdinalIgnoreCase))
            return;

        Load();
        OnReload();
    }
}