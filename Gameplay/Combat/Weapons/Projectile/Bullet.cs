using System.Collections.Generic;
using ContentLibrary;
using Gameplay.CollisionDetection;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Rendering;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies, ISpriteVisual
{
    private readonly HashSet<EnemyBase> _hitEnemies = [];
    private readonly HashSet<EnemyBase> _immuneEnemies;
    private readonly IEnumerable<IOnHitEffect> _onHits;
    private readonly int _pierce;

    private float _distanceTraveled = 0f;

    public Bullet(PlayerCharacter owner, Vector2 initialPosition, Vector2 velocity, float damage, float maxRange,
        int pierceEnemies = 0, IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null) : base(
        initialPosition)
    {
        Owner = owner;
        MaxRange = maxRange;
        _pierce = pierceEnemies;
        _onHits = onHits ?? [];
        _immuneEnemies = immuneEnemies ?? [];

        Velocity = velocity;
        Damage = damage;
    }

    /// <param name="initialPosition">Spawn the bullet here</param>
    /// <param name="target">Aim at this</param>
    /// <param name="damage">Deal this much damage on hit</param>
    /// <param name="maxRange">Expire after travelling this many pixels</param>
    /// <param name="pierceEnemies">Pierce this many enemies</param>
    /// <param name="onHit">Applied on hitting an enemy</param>
    public Bullet(PlayerCharacter owner, Vector2 initialPosition, Vector2 target, float damage, float maxRange,
        int pierceEnemies = 0, float speed = 1f, IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null) :
        this(owner, initialPosition, CalculateVelocity(initialPosition, target, speed), damage, maxRange, pierceEnemies,
            onHits, immuneEnemies) { }


    internal PlayerCharacter Owner { get; }
    internal float MaxRange { get; }
    public float Damage { get; }
    public ICollider Collider => new CircleCollider(this, 16f);

    public void OnHit(EnemyBase enemy)
    {
        if (_immuneEnemies.Contains(enemy)) return;

        enemy.TakeDamage(Owner, Damage);
        _hitEnemies.Add(enemy);
        if (_hitEnemies.Count > _pierce)
            MarkedForDeletion = true;

        foreach (var onHit in _onHits) onHit.Apply(this, enemy);
    }
    public float Layer => Layers.Projectiles;

    public string TexturePath => Paths.Images.Bullet;

    private static Vector2 CalculateVelocity(Vector2 initialPosition, Vector2 target, float speed) =>
        (Vector2)new UnitVector2(target - initialPosition) * speed;

    public override void Update(GameTime gameTime)
    {
        var previousPosition = Position;
        base.Update(gameTime);

        _distanceTraveled += Vector2.Distance(previousPosition, Position);
        if (_distanceTraveled >= MaxRange)
            MarkedForDeletion = true;
    }
}