using Entities.Combat;

namespace Entities.Enemy;

public abstract class EnemyBase(Vector2 position) : MovableEntity(position), IDamageableEnemy, IDamagesPlayer
{
    public abstract float Health { get; set; }
    public abstract float CollisionRadius { get; }
    public abstract float Damage { get; }
}