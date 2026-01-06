using System;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Entities.Pooling;

namespace Gameplay.Combat.Weapons.Projectile;

public class SniperRifle(
    PlayerCharacter owner,
    ISpawnEntity spawnEntity,
    IEntityFinder entityFinder,
    IAudioPlayer audio,
    BulletPool pool,
    CritCalculator critCalculator) : GunBase(owner.WeaponBelt.Stats)
{
    protected override TimeSpan Cooldown { get; } = TimeSpan.FromSeconds(2.8);
    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 3f;
        var baseDamage = 24f * Stats.DamageMultiplier;
        var damage = critCalculator.CalculateCritDamage(baseDamage, Stats, 2f);
        var range = 1000f * Stats.RangeMultiplier;

        var pierce = Stats.Pierce + 1;
        var bullet = pool.Get(owner, owner.Position, target.Position, bulletSpeed * Stats.SpeedMultiplier, damage,
            range, pierce, owner.WeaponBelt.OnHitEffects);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.SniperShoot);
    }
}