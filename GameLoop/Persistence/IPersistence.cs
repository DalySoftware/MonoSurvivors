using System;

namespace GameLoop.Persistence;

public interface IPersistence
{
    /// <param name="data">Serializable data to save</param>
    /// <param name="storageKey">Uniquely identify the stored data</param>
    public void Save<T>(T data, string storageKey);

    /// <param name="storageKey">Uniquely identify the stored data</param>
    public T Load<T>(string storageKey);

    /// <summary>
    ///     Event raised when data is saved, allowing IOptionsMonitor to detect changes
    /// </summary>
    event Action<string>? OnChanged;
}