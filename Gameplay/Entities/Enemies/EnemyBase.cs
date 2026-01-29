using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Entities.Effects;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public abstract class EnemyBase(Vector2 position, EnemyStats stats, EnemyDeathHandler deathHandler)
    : MovableEntity(position), IDamagesPlayer, IHasDrawTransform, IHasHitFlash
{
    private int _isDead; // Marker for concurrency
    private readonly List<SlowdownInstance> _activeSlows = [];
    private float _currentSlowMultiplier = 1f;
    private readonly HitSquash _hitSquash = new();
    private readonly HitFlash _hitFlash = new();

    public float Health { get; private set; } = stats.MaxHealth;
    public EnemyStats Stats { get; } = stats;
    public float Layer => Layers.Enemies;

    internal Vector2 SeparationForce { get; set; }

    internal float ApproximateRadius
    {
        get
        {
            if (field >= 0f)
                return field;

            var max = 0f;
            for (var i = 0; i < Colliders.Length; i++)
            {
                var r = Colliders[i].ApproximateRadius;
                if (r > max) max = r;
            }

            field = max;
            return max;
        }
    } = -1f;

    public float Experience => Stats.Experience;
    public int Damage => Stats.Damage;
    public required ICollider[] Colliders { get; init; }
    public Vector2 DrawScale => _hitSquash.Scale;
    public float FlashIntensity => _hitFlash.Intensity;
    public Color FlashColor => _hitFlash.Color;
    protected virtual void OnDeath(EnemyBase enemy) => deathHandler.ProcessDeath(enemy);

    public void TakeDamage(PlayerCharacter damager, float amount)
    {
        Health -= amount;

        // Trigger squash on any hit (including lethal)
        _hitSquash.Trigger(Position, damager.Position, amount, Stats.MaxHealth);
        _hitFlash.Trigger(amount, Stats.MaxHealth, Health <= 0f);

        if (Health > 0) return;

        if (Interlocked.Exchange(ref _isDead, 1) == 1)
            return; // another thread already processed death

        MarkedForDeletion = true;
        damager.OnKill(this);
        OnDeath(this);
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

        _hitSquash.Update(gameTime);
        _hitFlash.Update(gameTime);
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