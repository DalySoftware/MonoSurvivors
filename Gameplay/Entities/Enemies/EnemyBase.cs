using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public abstract class EnemyBase(Vector2 position, EnemyStats stats)
    : MovableEntity(position), IDamagesPlayer
{
    private int _isDead; // Marker for concurrency
    private readonly List<SlowdownInstance> _activeSlows = [];
    private float _currentSlowMultiplier = 1f;

    public float Health { get; private set; } = stats.MaxHealth;
    public EnemyStats Stats { get; } = stats;
    public float Layer => Layers.Enemies;

    internal List<EnemyBase> NearbyEnemies { get; } = new(16);

    protected Action<EnemyBase>? OnDeath { get; init; } = null;

    public float Experience => Stats.Experience;
    public int Damage => Stats.Damage;
    public required ICollider[] Colliders { get; init; }

    public void TakeDamage(PlayerCharacter damager, float amount)
    {
        Health -= amount;
        if (Health > 0) return;

        if (Interlocked.Exchange(ref _isDead, 1) == 1)
            return; // another thread already processed death

        MarkedForDeletion = true;
        damager.OnKill(this);
        OnDeath?.Invoke(this);
    }

    public void ApplyKnockback(Vector2 impulse)
    {
        if (Stats.KnockbackMultiplier <= 0)
            return;

        ExternalVelocity += impulse * Stats.KnockbackMultiplier;
    }

    /// <summary>
    ///     Slow enemy movement
    /// </summary>
    /// <param name="amount">Proportional slow. 1f = 100%, 0.3f = 30%</param>
    internal void ApplySlowdown(MovementSlowdown slowdown, GameTime gameTime)
    {
        var expiry = gameTime.TotalGameTime + slowdown.Duration;
        _activeSlows.Add(new SlowdownInstance(slowdown.Amount, expiry));
        RecalculateSlow();
    }

    public override void Update(GameTime gameTime)
    {
        UpdateActiveSlowdowns(gameTime);
        IntentVelocity *= _currentSlowMultiplier;

        base.Update(gameTime);

        const float knockbackDamping = 0.005f;

        ExternalVelocity = Vector2.Lerp(
            ExternalVelocity,
            Vector2.Zero,
            knockbackDamping * (float)gameTime.ElapsedGameTime.TotalMilliseconds
        );
    }

    private void UpdateActiveSlowdowns(GameTime gameTime)
    {
        var removedAny = false;

        for (var i = _activeSlows.Count - 1; i >= 0; i--)
            if (_activeSlows[i].Expiry <= gameTime.TotalGameTime)
            {
                _activeSlows.RemoveAt(i);
                removedAny = true;
            }

        if (removedAny)
            RecalculateSlow();
    }

    private void RecalculateSlow() =>
        _currentSlowMultiplier = 1f - _activeSlows.Select(s => s.Amount).DefaultIfEmpty(0f).Max();


    private readonly record struct SlowdownInstance(float Amount, TimeSpan Expiry);
}

public record EnemyStats(float MaxHealth, float Experience, int Damage, float KnockbackMultiplier = 1f);