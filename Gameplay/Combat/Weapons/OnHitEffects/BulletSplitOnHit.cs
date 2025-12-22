using System;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public sealed class BulletSplitOnHit(ISpawnEntity spawnEntity) : IOnHitEffect
{
    /// <summary>
    ///     How wide an arc should be covered, in radians.
    /// </summary>
    private const float ArcAngle = MathF.PI / 6;

    /// <summary>
    ///     Proportion of the original bullets damage that split bullets deal
    /// </summary>
    private const float DamageRatio = 0.3f;

    public int Priority => 0;

    public void Apply(IHitContext hitContext)
    {
        if (hitContext is not BulletHitContext context) return;
        var bullet = context.Bullet;
        var enemy = context.Enemy;

        var stats = bullet.Owner.WeaponBelt.Stats;
        if (stats.BulletSplit <= 0)
            return;

        var spawnPoint = enemy.Position;
        var range = bullet.MaxRange * 0.5f; // this will already have taken stats into account

        var bulletDirections = ArcSpreader.Arc(bullet.Velocity, stats.BulletSplit, ArcAngle);

        foreach (var direction in bulletDirections)
        {
            var speed = bullet.Velocity.Length();

            var splitBullet = new Bullet(
                bullet.Owner,
                spawnPoint,
                velocity: direction * speed,
                bullet.Damage * DamageRatio,
                range,
                immuneEnemies: [enemy]);

            spawnEntity.Spawn(splitBullet);
        }
    }
}