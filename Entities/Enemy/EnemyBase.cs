using System;
using Entities.Combat;
using Entities.Levelling;

namespace Entities.Enemy;

public abstract class EnemyBase(Vector2 position)
    : MovableEntity(position), IDamageableEnemy, IDamagesPlayer, ICreatesExperienceOnDeath
{
    /// <summary>
    ///     Will be executed with the current instance as the argument
    /// </summary>
    internal Action<EnemyBase> OnDeath { get; init; } = _ => { };

    public abstract float Experience { get; }

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

    public float CollisionRadius { get; init; }
    public float Damage { get; init; }
}