using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies, IVisual
{
    public Bullet(Vector2 initialPosition, Vector2 target, float damage) : base(initialPosition)
    {
        Velocity = (Vector2)new UnitVector2(target - initialPosition) * 1f;
        Damage = damage;
    }

    public float Damage { get; }
    public float CollisionRadius => 8f;

    public void OnHit() => MarkedForDeletion = true;
    public string TexturePath => Paths.Images.Bullet;
}