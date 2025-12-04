using Entities.Utilities;

namespace Entities.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies
{
    public Bullet(Vector2 initialPosition, Vector2 target) : base(initialPosition)
    {
        Velocity = (Vector2)new UnitVector2(target - initialPosition) * 1f;
    }

    public float Damage => 10f;
    public float CollisionRadius => 8f;
}