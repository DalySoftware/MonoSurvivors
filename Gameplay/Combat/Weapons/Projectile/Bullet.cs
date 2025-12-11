using ContentLibrary;
using Gameplay.Entities;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies, IVisual
{
    private readonly float _maxRange;

    private float _distanceTraveled = 0f;
    private int _enemyPiercesLeft = 0;

    /// <param name="initialPosition">Spawn the bullet here</param>
    /// <param name="target">Aim at this</param>
    /// <param name="damage">Deal this much damage on hit</param>
    /// <param name="maxRange">Expire after travelling this many pixels</param>
    /// <param name="pierceEnemies">Pierce this many enemies</param>
    public Bullet(Vector2 initialPosition, Vector2 target, float damage, float maxRange, int pierceEnemies = 0,
        float speed = 1f) : base(
        initialPosition)
    {
        Velocity = (Vector2)new UnitVector2(target - initialPosition) * speed;
        Damage = damage;
        _maxRange = maxRange;
        _enemyPiercesLeft = pierceEnemies;
    }

    public float Damage { get; }
    public float CollisionRadius => 16f;

    public void OnHit()
    {
        _enemyPiercesLeft--;

        if (_enemyPiercesLeft <= 0)
            MarkedForDeletion = true;
    }

    public string TexturePath => Paths.Images.Bullet;

    public override void Update(GameTime gameTime)
    {
        var previousPosition = Position;
        base.Update(gameTime);

        _distanceTraveled += Vector2.Distance(previousPosition, Position);
        if (_distanceTraveled >= _maxRange)
            MarkedForDeletion = true;
    }
}