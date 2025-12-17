using System.Collections.Generic;
using System.Threading;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Levelling;
using Gameplay.Rendering;

namespace Gameplay.Entities.Enemies;

public abstract class EnemyBase(Vector2 position, int damage)
    : MovableEntity(position), IDamagesPlayer, ICreatesExperienceOnDeath
{
    private int _isDead; // Marker for concurrency

    internal IEnumerable<EnemyBase> NearbyEnemies { get; set; } = [];
    public float Layer => Layers.Enemies;

    public float Health { get; protected set; }

    public abstract float Experience { get; }

    public int Damage { get; } = damage;
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