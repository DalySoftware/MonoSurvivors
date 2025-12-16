using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons;

internal class BulletSplitter(PlayerCharacter owner, float arcAngle, float damageRatio, ISpawnEntity spawnEntity)
{
    internal void SpawnAnyOnHitBulletSplits(Bullet originalBullet, EnemyBase enemy)
    {
        var stats = owner.WeaponBelt.Stats;
        if (stats.BulletSplit <= 0)
            return;

        var spawnPoint = enemy.Position;
        var range = originalBullet.MaxRange * 0.5f; // this will already have taken stats into account

        var bulletDirections = ArcSpreader.Arc(originalBullet.Velocity, stats.BulletSplit, arcAngle);

        foreach (var direction in bulletDirections)
        {
            var speed = originalBullet.Velocity.Length();

            var splitBullet = new Bullet(
                owner,
                spawnPoint,
                velocity: direction * speed,
                originalBullet.Damage * damageRatio,
                range,
                immuneEnemies: [enemy]);

            spawnEntity.Spawn(splitBullet);
        }
    }
}