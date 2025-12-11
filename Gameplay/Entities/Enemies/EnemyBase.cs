using System;
using System.Collections.Generic;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Levelling;

namespace Gameplay.Entities.Enemies;

public abstract class EnemyBase(Vector2 position, float collisionRadius, int damage)
    : MovableEntity(position), ICircleCollider, IDamagesPlayer, ICreatesExperienceOnDeath
{
    /// <summary>
    ///     Will be executed with the current instance as the argument
    /// </summary>
    internal Action<EnemyBase> OnDeath { get; init; } = _ => { };

    internal IEnumerable<EnemyBase> NearbyEnemies { get; set; } = [];

    public float Health
    {
        get;
        set
        {
            if (value <= 0)
            {
                MarkedForDeletion = true;
                OnDeath(this);
            }

            field = value;
        }
    }

    public float CollisionRadius { get; } = collisionRadius;

    public abstract float Experience { get; }
    public int Damage { get; } = damage;
}