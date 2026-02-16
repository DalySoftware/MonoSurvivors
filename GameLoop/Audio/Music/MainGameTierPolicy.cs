using System.Linq;
using Gameplay.Entities;
using Gameplay.Entities.Enemies.Spawning;

namespace GameLoop.Audio.Music;

internal sealed class MainGameTierPolicy(IEntityFinder entities, PlayerCharacter player, EnemySpawner spawner)
    : IMusicTierPolicy
{
    // Spend at least this time in a tier before moving
    private const int MinDwellBoundaries = 2;

    private int _boundariesInTier;

    private readonly EnemyHpSignal _enemyHp = new(entities);

    private int _breatherCooldown;

    public void OnLoopBoundary()
    {
        _boundariesInTier++;

        if (_breatherCooldown > 0)
            _breatherCooldown--;
    }

    public MusicTier DecideTier(MusicTier currentTier)
    {
        // --- sample signals once per boundary ---
        var hpSample = _enemyHp.Sample();

        if (_boundariesInTier < MinDwellBoundaries)
            return currentTier;

        // 1) Pressure-suggested tier (HP hysteresis)
        var suggested = currentTier switch
        {
            MusicTier.Soft when hpSample.Smoothed >= 520 => MusicTier.Core,
            MusicTier.Core when hpSample.Smoothed <= 400 => MusicTier.Soft,

            MusicTier.Core when hpSample.Smoothed >= 1400 => MusicTier.Peak,
            MusicTier.Peak when hpSample.Smoothed <= 1100 => MusicTier.Core,

            _ => currentTier,
        };

        // 2) Momentum adjustment: avoid sticking in Peak when pressure is stable/falling.
        // If HP isn't rising meaningfully, drop to Core.
        if (suggested == MusicTier.Peak && hpSample.SmoothedDelta <= 15f)
            suggested = MusicTier.Core;

        // 2b) Player HP bias: panic pushes *up*.
        suggested = ApplyPlayerHpBias(suggested);

        // 2c) Boss bias: boss presence forces Peak.
        var bossActive = spawner.ActiveBosses.Count > 0;
        if (bossActive)
            suggested = MusicTier.Peak;

        // 3) Breathers: short dips so we don't sit in one tier forever
        var playerIsUnderPressure = player.Health <= 2 || hpSample.Smoothed > 2000f || bossActive;
        const int breatherNeededAfter = 16;
        var skipBreather = playerIsUnderPressure ||
                           _breatherCooldown > 0 ||
                           _boundariesInTier < breatherNeededAfter ||
                           suggested == MusicTier.Soft;
        if (skipBreather)
            return CommitTierChange(currentTier, suggested);

        _breatherCooldown = 4;
        return CommitTierChange(currentTier, OneStepDown(suggested));
    }

    private MusicTier ApplyPlayerHpBias(MusicTier suggested) => player.Health switch
    {
        1 => MusicTier.Peak,
        2 when suggested == MusicTier.Soft => MusicTier.Core,
        _ => suggested,
    };

    private MusicTier CommitTierChange(MusicTier currentTier, MusicTier nextTier)
    {
        if (nextTier == currentTier)
            return currentTier;

        _boundariesInTier = 0;
        return nextTier;
    }

    private static MusicTier OneStepDown(MusicTier tier) => tier switch
    {
        MusicTier.Peak => MusicTier.Core,
        MusicTier.Core => MusicTier.Soft,
        _ => tier,
    };

    /// <summary>
    ///     Computes a smoothed proxy for combat pressure based on total enemy HP,
    ///     plus a per-boundary trend (delta) of that smoothed value.
    ///     Sampled once per loop boundary.
    /// </summary>
    private sealed class EnemyHpSignal(IEntityFinder entities)
    {
        private const float Smoothing = 0.35f;
        private float? _smoothed;

        private float _prevSmoothed;
        private float _smoothedDelta;

        public SampleResult Sample()
        {
            var raw = entities.Enemies.Sum(e => e.Health);

            var prev = _smoothed ?? raw;
            var next = prev + (raw - prev) * Smoothing;

            _smoothed = next;

            var delta = next - _prevSmoothed;
            _prevSmoothed = next;
            _smoothedDelta = delta;

            return new SampleResult(next, _smoothedDelta);
        }

        public readonly struct SampleResult(float smoothed, float smoothedDelta)
        {
            public float Smoothed { get; } = smoothed;
            public float SmoothedDelta { get; } = smoothedDelta;
        }
    }
}