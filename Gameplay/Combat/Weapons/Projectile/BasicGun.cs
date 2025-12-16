using System;
using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : GunBase(owner.WeaponBelt.Stats)
{
    private readonly BulletSplitter _bulletSplitter = new(owner, MathF.PI / 6, 0.5f, spawnEntity);

    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 1f;
        var baseDamage = 8f * Stats.DamageMultiplier;
        var damage = CritCalculator.CalculateDamage(baseDamage, Stats.CritChance, Stats.CritDamage);
        var range = 300f * Stats.RangeMultiplier;

        var bullet = new Bullet(owner, owner.Position, target.Position, damage, range, Stats.Pierce,
            bulletSpeed * Stats.SpeedMultiplier, _bulletSplitter.SpawnAnyOnHitBulletSplits);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
}