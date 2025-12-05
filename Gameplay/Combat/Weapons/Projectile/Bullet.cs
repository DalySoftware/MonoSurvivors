using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies, IVisual
{
    public Bullet(Vector2 initialPosition, Vector2 target) : base(initialPosition)
    {
        Velocity = (Vector2)new UnitVector2(target - initialPosition) * 1f;
    }

    public float Damage => 10f;
    public float CollisionRadius => 8f;
    public string TexturePath => Paths.Images.Bullet;

    public void OnHit() => MarkedForDeletion = true;
}