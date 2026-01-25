using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Entities.Pooling;

public class BulletPool
{
    private readonly Stack<Bullet> _pool = new();

    public Bullet Get(
        BulletType type,
        PlayerCharacter owner,
        Vector2 position,
        Vector2 target,
        float speed,
        float damage,
        float range,
        IReadOnlyList<IOnHitEffect> onHits,
        int pierce = 0,
        HashSet<EnemyBase>? immuneEnemies = null)
    {
        var velocity = VectorCalculations.Velocity(position, target, speed);
        return Get(type, owner, position, velocity, damage, range, onHits, pierce, immuneEnemies);
    }

    public Bullet Get(
        BulletType type,
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float range,
        IReadOnlyList<IOnHitEffect> onHits,
        int pierce = 0,
        HashSet<EnemyBase>? immuneEnemies = null)
    {
        var (radius, sprite) = GetTypeSpecificValues(type);
        if (_pool.TryPop(out var bullet))
            return bullet.Reinitialize(owner, position, velocity, damage, range, radius, sprite, pierce, onHits,
                immuneEnemies);

        return Create(owner, position, velocity, damage, range, radius, sprite, pierce, onHits, immuneEnemies);
    }

    private Bullet Create(
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float range,
        float radius,
        string spritePath,
        int pierce,
        IReadOnlyList<IOnHitEffect> onHits,
        HashSet<EnemyBase>? immuneEnemies) =>
        new(this, owner, position, velocity, damage, range, radius, spritePath, onHits, immuneEnemies, pierce);
    private static (float radius, string spritePath) GetTypeSpecificValues(BulletType type) => type switch
    {
        BulletType.Basic => (16f, Paths.Images.Bullets.Basic),
        BulletType.BasicSmall => (12f, Paths.Images.Bullets.BasicSmall),
        BulletType.Bouncer => (24f, Paths.Images.Bullets.Bouncer),
        BulletType.Sniper => (16f, Paths.Images.Bullets.Sniper),
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };


    internal void Return(Bullet bullet) => _pool.Push(bullet);
}

public enum BulletType
{
    Basic,
    BasicSmall,
    Bouncer,
    Sniper,
}