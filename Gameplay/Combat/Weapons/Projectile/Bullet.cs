using System;
using System.Collections.Generic;
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
    private readonly CircleCollider _circle;

    private float _distanceTraveled = 0f;

    private IOnHitEffect[] _onHitEffects = [];

    public Bullet(BulletPool pool, PlayerCharacter owner, Vector2 initialPosition, Vector2 velocity, float damage,
        float maxRange, float radius, string texturePath,
        IReadOnlyList<IOnHitEffect> onHits, HashSet<EnemyBase>? immuneEnemies,
        int pierceEnemies = 0) : base(initialPosition)
    {
        Owner = owner;
        MaxRange = maxRange;
        _pool = pool;
        TexturePath = texturePath;
        _piercesLeft = pierceEnemies;
        SetOnHitEffects(onHits);
        _immuneEnemies = immuneEnemies ?? [];

        IntentVelocity = velocity;
        Damage = damage;
        _circle = new CircleCollider(this, radius);
        Colliders = [_circle];
    }

    internal PlayerCharacter Owner { get; private set; }
    internal float MaxRange { get; private set; }
    internal float RemainingRange => MaxRange - _distanceTraveled;

    public float Damage { get; private set; }
    public ICollider[] Colliders { get; }

    public void OnHit(GameTime gameTime, EnemyBase enemy)
    {
        if (!_immuneEnemies.Add(enemy))
            return;

        enemy.TakeDamage(Owner, Damage);

        var context = new BulletHitContext(gameTime, Owner, enemy, this);

        for (var i = 0; i < _onHitEffects.Length; i++)
            _onHitEffects[i].Apply(context);

        Resolve(context);
    }

    public void OnDespawned() => _pool.Return(this);

    public float Layer => Layers.Projectiles;

    public string TexturePath { get; private set; }

    private void SetOnHitEffects(IReadOnlyList<IOnHitEffect> value)
    {
        if (value.Count == 0)
        {
            _onHitEffects = [];
            return;
        }

        var arr = new IOnHitEffect[value.Count];
        for (var i = 0; i < value.Count; i++) arr[i] = value[i];

        Array.Sort(arr, static (a, b) => a.Priority.CompareTo(b.Priority));
        _onHitEffects = arr;
    }

    public Bullet Reinitialize(
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float maxRange,
        float radius,
        string texturePath,
        int pierceEnemies,
        IReadOnlyList<IOnHitEffect> onHits,
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

        SetOnHitEffects(onHits);
        if (immuneEnemies != null) _immuneEnemies = immuneEnemies;
        else _immuneEnemies.Clear();

        TexturePath = texturePath;
        _circle.CollisionRadius = radius;

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