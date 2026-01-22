using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLoop.Audio;

public sealed class MusicDucker
{
    private readonly object _lock = new();

    private float _baseVolume = 1f; // 0..1
    private float _manualDuck = 1f; // 0..1
    private long _nextId = 1;

    // Temporary ducks (e.g. gunshots) keyed by id -> factor.
    private readonly Dictionary<long, float> _tempDucks = new();

    public float EffectiveVolume { get; private set; } = 1f;

    public event Action<float>? OnEffectiveVolumeChanged;

    public void SetBaseVolume(float baseVolume01)
    {
        lock (_lock)
        {
            _baseVolume = ClampZeroToOne(baseVolume01);
            RecomputeAndNotifyIfChanged();
        }
    }

    public void SetManualDuck(float factor01)
    {
        lock (_lock)
        {
            _manualDuck = ClampZeroToOne(factor01);
            RecomputeAndNotifyIfChanged();
        }
    }

    public void ClearManualDuck() => SetManualDuck(1f);

    public DuckHandle BeginTemporaryDuck(float factor01)
    {
        lock (_lock)
        {
            var id = _nextId++;
            _tempDucks[id] = ClampZeroToOne(factor01);
            RecomputeAndNotifyIfChanged();
            return new DuckHandle(this, id);
        }
    }

    private void EndTemporaryDuck(long id)
    {
        lock (_lock)
        {
            if (_tempDucks.Remove(id))
                RecomputeAndNotifyIfChanged();
        }
    }

    private void RecomputeAndNotifyIfChanged()
    {
        var tempMin = _tempDucks.Select(kv => kv.Value).Prepend(1f).Min();

        var duck = Math.Min(_manualDuck, tempMin);
        var effective = ClampZeroToOne(_baseVolume * duck);

        if (NearlyEqual(effective, EffectiveVolume))
            return;

        EffectiveVolume = effective;
        OnEffectiveVolumeChanged?.Invoke(EffectiveVolume);
    }

    private static float ClampZeroToOne(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

    private static bool NearlyEqual(float a, float b) => Math.Abs(a - b) < 0.0005f;

    public sealed class DuckHandle : IDisposable
    {
        private MusicDucker? _owner;
        private readonly long _id;

        internal DuckHandle(MusicDucker owner, long id)
        {
            _owner = owner;
            _id = id;
        }

        public void Dispose()
        {
            var owner = _owner;
            if (owner == null) return;

            _owner = null;
            owner.EndTemporaryDuck(_id);
        }
    }
}