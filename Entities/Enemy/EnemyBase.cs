using Entities.Combat;

namespace Entities.Enemy;

public abstract class EnemyBase(Vector2 position) : MovableEntity(position), IDamageableEnemy, IDamagesPlayer
{
    public float Health
    {
        get;
        set
        {
            if (value <= 0)
                MarkedForDeletion = true;
            field = value;
        }
    }

    public float CollisionRadius { get; init; }
    public float Damage { get; init; }
}