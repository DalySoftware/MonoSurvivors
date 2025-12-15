using System;
using System.Collections.Generic;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Levelling;

namespace Gameplay.Entities.Enemies;

public abstract class EnemyBase(Vector2 position, int damage)
    : MovableEntity(position), IDamagesPlayer, ICreatesExperienceOnDeath
{
    /// <summary>
    ///     Will be executed with the current instance as the argument
    /// </summary>
    internal Action<EnemyBase, PlayerCharacter>? OnDeath { get; init; }

    internal IEnumerable<EnemyBase> NearbyEnemies { get; set; } = [];

    public float Health { get; protected set; }

    public abstract float Experience { get; }

    public int Damage { get; } = damage;
    public required ICollider Collider { get; init; }

    public void TakeDamage(PlayerCharacter damager, float amount)
    {
        Health -= amount;
        if (Health > 0) return;

        MarkedForDeletion = true;
        OnDeath?.Invoke(this, damager);
    }
}