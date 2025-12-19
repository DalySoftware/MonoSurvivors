using System;
using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Combat.Weapons.Projectile;

public class SniperRifle(
    PlayerCharacter owner,
    ISpawnEntity spawnEntity,
    IEntityFinder entityFinder,
    IAudioPlayer audio) : GunBase(owner.WeaponBelt.Stats)
{
    protected override TimeSpan Cooldown { get; } = TimeSpan.FromSeconds(3);
    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 3f;
        var baseDamage = 24f * Stats.DamageMultiplier;
        var damage = CritCalculator.CalculateDamage(baseDamage, Stats.CritChance, Stats.CritDamage);
        var range = 1000f * Stats.RangeMultiplier;

        var bullet = new Bullet(owner, owner.Position, target.Position, damage, range, Stats.Pierce,
            bulletSpeed * Stats.SpeedMultiplier, owner.WeaponBelt.OnHitEffects);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
}