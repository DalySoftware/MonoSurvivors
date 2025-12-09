using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies, IVisual
{
    public float Damage { get; }
    public float CollisionRadius => 16f;
    public string TexturePath => Paths.Images.Bullet;
    public void OnHit() => MarkedForDeletion = true;
    
    public Bullet(Vector2 initialPosition, Vector2 target, float damage, float maxRange) : base(initialPosition)
    {
        Velocity = (Vector2)new UnitVector2(target - initialPosition) * 1f;
        Damage = damage;
        _maxRange = maxRange;
    }

    private float _distanceTraveled = 0f;
    private float _maxRange;

    public override void Update(GameTime gameTime)
    {
        var previousPosition = Position;
        base.Update(gameTime);
        
        _distanceTraveled += Vector2.Distance(previousPosition, Position);
        if (_distanceTraveled >= _maxRange)
            MarkedForDeletion = true;
    }
}