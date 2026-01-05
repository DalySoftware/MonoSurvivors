using System;
using System.Text.Json.Serialization.Metadata;
using GameLoop.Persistence;

namespace GameLoop.UserSettings;

public sealed class MergingSettingsPersistence(ISettingsPersistence inner) : ISettingsPersistence
{
    public void Save<T>(T data, JsonTypeInfo<T> typeInfo) => inner.Save(data, typeInfo);

    public T Load<T>(JsonTypeInfo<T> typeInfo)
        where T : new()
    {
        var defaults = new T();
        var overrides = inner.Load(typeInfo);

        if (defaults is not IMergeable<T> mergeable) return overrides;

        // Merge if supported
        mergeable.MergeFrom(overrides);
        return defaults;
    }

    public event Action<Type>? OnChanged
    {
        add => inner.OnChanged += value;
        remove => inner.OnChanged -= value;
    }
}

public interface IMergeable<in T>
{
    void MergeFrom(T overrides);
}