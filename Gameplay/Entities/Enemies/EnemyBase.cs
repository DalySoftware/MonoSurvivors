using System.Collections.Generic;
using System.Threading;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Levelling;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public abstract class EnemyBase(Vector2 position, EnemyStats stats)
    : MovableEntity(position), IDamagesPlayer, ICreatesExperienceOnDeath
{
    private int _isDead; // Marker for concurrency

    public float Health { get; private set; } = stats.MaxHealth;
    public EnemyStats Stats { get; } = stats;
    public float Layer => Layers.Enemies;
    internal IEnumerable<EnemyBase> NearbyEnemies { get; set; } = [];
    public float Experience => Stats.Experience;
    public int Damage => Stats.Damage;
    public required ICollider Collider { get; init; }

    public void TakeDamage(PlayerCharacter damager, float amount)
    {
        Health -= amount;
        if (Health > 0) return;

        if (Interlocked.Exchange(ref _isDead, 1) == 1)
            return; // another thread already processed death

        MarkedForDeletion = true;
        damager.OnKill(this);
    }
}

public record EnemyStats(float MaxHealth, float Experience, int Damage);