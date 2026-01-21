using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GameLoop.Persistence;
using Microsoft.JSInterop;

namespace Veil.Web.PlatformServices.Persistence;

public class LocalStoragePersistence(IJSInProcessRuntime jsRuntime) : ISettingsPersistence
{
    private const string Prefix = "MonoSurvivors:"; // change if you want

    public void Save<T>(T data, JsonTypeInfo<T> typeInfo)
    {
        var key = GetStorageKey<T>();
        var json = JsonSerializer.Serialize(data, typeInfo);

        jsRuntime.InvokeVoid("localStorage.setItem", key, json);
        OnChanged?.Invoke(typeof(T));
    }

    public T Load<T>(JsonTypeInfo<T> typeInfo) where T : new()
    {
        var key = GetStorageKey<T>();
        var json = jsRuntime.Invoke<string?>("localStorage.getItem", key);

        if (string.IsNullOrWhiteSpace(json))
            return new T();

        return JsonSerializer.Deserialize(json, typeInfo) ?? new T();
    }

    public event Action<Type>? OnChanged;

    private static string GetStorageKey<T>()
    {
        var name = (typeof(T).FullName ?? typeof(T).Name) + ".json";
        return Prefix + SanitizeKey(name);
    }

    // localStorage keys can be basically anything, but this keeps it consistent with your desktop approach.
    private static string SanitizeKey(string key) =>
        Path.GetInvalidFileNameChars().Aggregate(key, (current, c) => current.Replace(c, '_'));
}