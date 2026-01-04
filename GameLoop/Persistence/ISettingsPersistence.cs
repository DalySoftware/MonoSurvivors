using System;
using System.Text.Json.Serialization.Metadata;

namespace GameLoop.Persistence;

public interface ISettingsPersistence
{
    /// <param name="data">Uniquely typed serializable data to save</param>
    /// <typeparam name="T">Unique type</typeparam>
    public void Save<T>(T data, JsonTypeInfo<T> typeInfo);

    /// <typeparam name="T">Unique type</typeparam>
    public T Load<T>(JsonTypeInfo<T> typeInfo)
        where T : new();

    /// <summary>
    ///     Event raised when data is saved. Subscribers receive the type that changed.
    /// </summary>
    event Action<Type>? OnChanged;
}