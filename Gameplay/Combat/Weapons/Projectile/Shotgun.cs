using System;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Shotgun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : GunBase(owner.WeaponBelt.Stats)
{
    private readonly BulletSplitter _bulletSplitter = new(owner, MathF.PI / 6, 0.5f, spawnEntity);

    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 1f;
        var baseDamage = 1f * Stats.DamageMultiplier;
        var damage = CritCalculator.CalculateDamage(baseDamage, Stats.CritChance, Stats.CritDamage);
        var range = 300f * Stats.RangeMultiplier;

        var targetDirection = target.Position - owner.Position;
        foreach (var direction in ArcSpreader.Arc(targetDirection, 5, MathF.PI / 6))
        {
            var velocity = direction * bulletSpeed * owner.WeaponBelt.Stats.SpeedMultiplier;
            var bullet = new Bullet(owner, owner.Position, velocity, damage, range, Stats.Pierce,
                _bulletSplitter.SpawnAnyOnHitBulletSplits);
            spawnEntity.Spawn(bullet);
        }

        audio.Play(SoundEffectTypes.Shoot); // todo
    }
}