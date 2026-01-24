using System;
using Gameplay.Audio;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Entities.Pooling;

namespace Gameplay.Combat.Weapons.Projectile;

public class BouncingGun(
    PlayerCharacter owner,
    ISpawnEntity spawnEntity,
    IEntityFinder entityFinder,
    IAudioPlayer audio,
    BulletPool pool,
    CritCalculator critCalculator)
    : GunBase(owner.WeaponBelt.Stats)
{
    private readonly static BounceOnHit BounceOnHit = new();

    protected override TimeSpan Cooldown { get; } = TimeSpan.FromSeconds(1);
    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 0.6f;
        var baseDamage = 4f * Stats.DamageMultiplier;
        var damage = critCalculator.CalculateCritDamage(baseDamage, Stats);
        var range = 600f * Stats.RangeMultiplier;

        var bullet = pool.Get(BulletType.Bouncer, owner, owner.Position, target.Position,
            bulletSpeed * Stats.SpeedMultiplier, damage,
            range, [BounceOnHit, ..owner.WeaponBelt.OnHitEffects], Stats.Pierce);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.BouncerShoot);
    }
}