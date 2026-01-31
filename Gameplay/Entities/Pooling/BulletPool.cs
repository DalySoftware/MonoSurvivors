using System;
using System.Collections.Generic;
using ContentLibrary;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Combat.Weapons.OnHitEffects.Visual;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities.Enemies;
using Gameplay.Rendering.Colors;
using Gameplay.Utilities;

namespace Gameplay.Entities.Pooling;

public class BulletPool(GritBurstOnHit gritBurst, BloomPuffOnHit bloomPuff)
{
    private readonly Stack<Bullet> _pool = new();
    private readonly IOnHitVisualEffect[] _bulletEffects = [gritBurst, bloomPuff];

    public Bullet Get(
        BulletType type,
        PlayerCharacter owner,
        Vector2 position,
        Vector2 target,
        float speed,
        float damage,
        float range,
        IReadOnlyList<IOnHitEffect> onHits,
        HashSet<EnemyBase>? immuneEnemies = null,
        int pierce = 0)
    {
        var velocity = VectorCalculations.Velocity(position, target, speed);
        return Get(type, owner, position, velocity, damage, range, onHits, immuneEnemies, pierce);
    }

    public Bullet Get(
        BulletType type,
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float range,
        IReadOnlyList<IOnHitEffect> onHits,
        HashSet<EnemyBase>? immuneEnemies = null,
        int pierce = 0)
    {
        var (radius, sprite, visualEffects, effectColor) = GetTypeSpecificValues(type);
        if (_pool.TryPop(out var bullet))
            return bullet.Reinitialize(owner, position, velocity, damage, range, radius, sprite, pierce, onHits,
                immuneEnemies, visualEffects);

        return Create(owner, position, velocity, damage, range, radius, sprite, effectColor, onHits, immuneEnemies,
            pierce, visualEffects);
    }

    private Bullet Create(
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float range,
        float radius,
        string spritePath,
        Color effectColor,
        IReadOnlyList<IOnHitEffect> onHits,
        HashSet<EnemyBase>? immuneEnemies,
        int pierce,
        IReadOnlyList<IOnHitVisualEffect>? onHitVisualEffects) =>
        new(this, owner, position, velocity, damage, range, radius, spritePath, effectColor, onHits, immuneEnemies,
            pierce,
            onHitVisualEffects);

    private (float radius, string spritePath, IReadOnlyList<IOnHitVisualEffect> onHitVisualEffects, Color effectColor)
        GetTypeSpecificValues(BulletType type) => type switch
    {
        BulletType.Basic => (16f, Paths.Images.Bullets.Basic, _bulletEffects, ColorPalette.Yellow),
        BulletType.BasicSmall => (12f, Paths.Images.Bullets.BasicSmall, _bulletEffects, ColorPalette.Orange),
        BulletType.Bouncer => (24f, Paths.Images.Bullets.Bouncer, _bulletEffects, ColorPalette.Green),
        BulletType.Sniper => (16f, Paths.Images.Bullets.Sniper, _bulletEffects, ColorPalette.Ice),
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