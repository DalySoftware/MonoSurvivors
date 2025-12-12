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

    public abstract float Experience { get; }

    public int Damage { get; } = damage;
    public required ICollider Collider { get; init; }
}