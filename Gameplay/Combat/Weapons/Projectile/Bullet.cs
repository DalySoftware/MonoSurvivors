using System.Collections.Generic;
using System.Linq;
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
    private readonly HashSet<EnemyBase> _immuneEnemies;
    private int _piercesLeft;

    private float _distanceTraveled = 0f;

    public Bullet(PlayerCharacter owner, Vector2 initialPosition, Vector2 velocity, float damage, float maxRange,
        int pierceEnemies = 0, IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null) : base(
        initialPosition)
    {
        Owner = owner;
        MaxRange = maxRange;
        _piercesLeft = pierceEnemies;
        OnHitEffects = onHits ?? [];
        _immuneEnemies = immuneEnemies ?? [];

        IntentVelocity = velocity;
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
    internal float RemainingRange => MaxRange - _distanceTraveled;
    internal IEnumerable<IOnHitEffect> OnHitEffects { get; }
    public float Damage { get; private set; }
    public ICollider Collider => new CircleCollider(this, 16f);

    public void OnHit(EnemyBase enemy)
    {
        if (_immuneEnemies.Contains(enemy)) return;

        enemy.TakeDamage(Owner, Damage);

        var context = new BulletHitContext(Owner, enemy, this);

        foreach (var onHit in OnHitEffects.OrderBy(o => o.Priority))
            onHit.Apply(context);

        Resolve(context);
    }

    public float Layer => Layers.Projectiles;

    public string TexturePath => Paths.Images.Bullet;

    private void Resolve(BulletHitContext ctx)
    {
        if (ctx.Delete)
        {
            MarkedForDeletion = true;
            return;
        }

        if (ctx is { Bounce: true, BounceVelocity: { } velocity, BounceDamageMultiplier: var multiplier })
        {
            IntentVelocity = velocity;
            Damage *= multiplier;
            _immuneEnemies.Add(ctx.Enemy);
            return;
        }

        if (ctx.ConsumePierce && _piercesLeft-- <= 0) // NB: 0 pierce is allowed to hit 1 enemy
            MarkedForDeletion = true;
    }

    private static Vector2 CalculateVelocity(Vector2 initialPosition, Vector2 target, float speed) =>
        (Vector2)new UnitVector2(target - initialPosition) * speed;

    public override void Update(GameTime gameTime)
    {
        var previousPosition = Position;
        base.Update(gameTime);

        _distanceTraveled += Vector2.Distance(previousPosition, Position);
        if (RemainingRange <= 0)
            MarkedForDeletion = true;
    }
}