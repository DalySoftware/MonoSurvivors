using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Manages the lifecycle of visual effects across all entities
/// </summary>
public class EffectManager
{
    private readonly Dictionary<object, List<VisualEffect>> _effectsByEntity = [];

    /// <summary>
    ///     Create a new effect for the given entity
    /// </summary>
    public void FireEffect<T>(T entity, VisualEffect effect)
        where T : notnull
    {
        if (!_effectsByEntity.ContainsKey(entity))
            _effectsByEntity[entity] = [];

        _effectsByEntity[entity].Add(effect);
    }

    public IReadOnlyList<VisualEffect> GetEffects<T>(T entity)
        where T : notnull =>
        _effectsByEntity.TryGetValue(entity, out var effects) ? effects.AsReadOnly() : [];

    /// <summary>
    ///     Update all effects and remove expired ones
    /// </summary>
    public void Update(GameTime gameTime)
    {
        foreach (var effect in _effectsByEntity.Values.SelectMany(effects => effects))
            effect.Update(gameTime);

        _effectsByEntity.RemoveValuesWhere(value => !value.IsActive);
        _effectsByEntity.RemoveKeysWhere(kvp => kvp.Value.Count == 0);
    }
}

internal static class DictionaryExtensions
{
    extension<TKey, TValue>(Dictionary<TKey, List<TValue>> dict) where TKey : notnull
    {
        internal void RemoveKeysWhere(Func<KeyValuePair<TKey, List<TValue>>, bool> filterPredicate)
        {
            var keysToRemove = dict
                .Where(filterPredicate)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
                dict.Remove(key);
        }

        internal void RemoveValuesWhere(Func<TValue, bool> valuePredicate)
        {
            foreach (var key in dict.Keys.ToList())
            {
                var values = dict[key];
                values.RemoveAll(v => valuePredicate(v));
            }
        }
    }
}