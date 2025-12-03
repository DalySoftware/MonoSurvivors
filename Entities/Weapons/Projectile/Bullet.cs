using Entities.Utilities;

namespace Entities.Weapons.Projectile;

public class Bullet : MovableEntity
{
    public Bullet(Vector2 initialPosition, Vector2 target) : base(initialPosition)
    {
        Velocity = (Vector2)new UnitVector2(target - initialPosition) * 1f;
    }
}