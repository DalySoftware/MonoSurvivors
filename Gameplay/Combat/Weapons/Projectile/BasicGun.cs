using System;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Entities.Pooling;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(
    PlayerCharacter owner,
    ISpawnEntity spawnEntity,
    IEntityFinder entityFinder,
    IAudioPlayer audio,
    BulletPool pool,
    CritCalculator critCalculator)
    : GunBase(owner.WeaponBelt.Stats)
{
    protected override TimeSpan Cooldown { get; } = TimeSpan.FromSeconds(.9);
    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 1f;
        var baseDamage = 8f * Stats.DamageMultiplier;
        var damage = critCalculator.CalculateCritDamage(baseDamage, Stats);
        var range = 300f * Stats.RangeMultiplier;

        var bullet = pool.Get(owner, owner.Position, target.Position, bulletSpeed * Stats.SpeedMultiplier, damage,
            range, Stats.Pierce, owner.WeaponBelt.OnHitEffects);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.BasicShoot);
    }
}