using System.Collections.Generic;
using System.Linq;
using ContentLibrary;
using Gameplay.CollisionDetection;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Entities.Pooling;
using Gameplay.Rendering;

namespace Gameplay.Combat.Weapons.Projectile;

public class Bullet : MovableEntity, IDamagesEnemies, ISpriteVisual, IPoolableEntity
{
    private HashSet<EnemyBase> _immuneEnemies;
    private readonly BulletPool _pool;
    private int _piercesLeft;

    private float _distanceTraveled = 0f;

    public Bullet(BulletPool pool, PlayerCharacter owner, Vector2 initialPosition, Vector2 velocity, float damage,
        float maxRange, int pierceEnemies = 0, IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null) : base(initialPosition)
    {
        Owner = owner;
        MaxRange = maxRange;
        _pool = pool;
        _piercesLeft = pierceEnemies;
        OnHitEffects = onHits ?? [];
        _immuneEnemies = immuneEnemies ?? [];

        IntentVelocity = velocity;
        Damage = damage;
    }

    internal PlayerCharacter Owner { get; private set; }
    internal float MaxRange { get; private set; }
    internal float RemainingRange => MaxRange - _distanceTraveled;
    internal IEnumerable<IOnHitEffect> OnHitEffects { get; private set; }
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

    public void OnDespawned() => _pool.Return(this);

    public float Layer => Layers.Projectiles;

    public string TexturePath => Paths.Images.Bullet;

    public Bullet Reinitialize(
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float maxRange,
        int pierceEnemies,
        IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null)
    {
        Owner = owner;
        Position = position;
        IntentVelocity = velocity;

        Damage = damage;
        MaxRange = maxRange;
        _piercesLeft = pierceEnemies;

        _distanceTraveled = 0f;
        MarkedForDeletion = false;

        _immuneEnemies = immuneEnemies ?? [];
        OnHitEffects = onHits ?? [];

        return this;
    }

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

    public override void Update(GameTime gameTime)
    {
        var previousPosition = Position;
        base.Update(gameTime);

        _distanceTraveled += Vector2.Distance(previousPosition, Position);
        if (RemainingRange <= 0)
            MarkedForDeletion = true;
    }
}