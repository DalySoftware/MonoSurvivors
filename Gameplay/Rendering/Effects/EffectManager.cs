using System;
using System.Collections.Generic;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Manages timed visual effects per entity.
///     Stores expiry separately so VisualEffect stays a stable dictionary key.
/// </summary>
public sealed class EffectManager
{
    private readonly List<object> _emptyKeys = [];
    private readonly Dictionary<object, List<ActiveEffect>> _effectsByEntity = new();

    public void FireEffect<T>(T entity, VisualEffect effect, GameTime gameTime, TimeSpan duration)
        where T : notnull
    {
        if (!_effectsByEntity.TryGetValue(entity, out var list))
        {
            list = new List<ActiveEffect>(2);
            _effectsByEntity.Add(entity, list);
        }

        var expiresAt = gameTime.TotalGameTime + duration;

        // Refresh if the same logical effect already exists
        for (var i = 0; i < list.Count; i++)
            if (Equals(list[i].Effect, effect))
            {
                if (expiresAt > list[i].ExpiresAt)
                    list[i] = list[i] with { ExpiresAt = expiresAt };
                return;
            }

        list.Add(new ActiveEffect(effect, expiresAt));
    }

    public IReadOnlyList<ActiveEffect> GetEffects<T>(T entity)
        where T : notnull =>
        _effectsByEntity.TryGetValue(entity, out var list) ? list : Array.Empty<ActiveEffect>();


    public void Update(GameTime gameTime)
    {
        var now = gameTime.TotalGameTime;
        _emptyKeys.Clear();

        foreach (var (entity, list) in _effectsByEntity)
        {
            for (var i = list.Count - 1; i >= 0; i--)
                if (list[i].ExpiresAt <= now)
                    list.RemoveAt(i);

            if (list.Count == 0)
                _emptyKeys.Add(entity);
        }

        for (var i = 0; i < _emptyKeys.Count; i++)
            _effectsByEntity.Remove(_emptyKeys[i]);
    }


    public readonly record struct ActiveEffect(VisualEffect Effect, TimeSpan ExpiresAt);
}