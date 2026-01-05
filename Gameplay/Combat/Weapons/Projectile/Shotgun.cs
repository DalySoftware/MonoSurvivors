using System;
using Gameplay.Audio;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Entities;
using Gameplay.Entities.Pooling;
using Gameplay.Utilities;

namespace Gameplay.Combat.Weapons.Projectile;

public class Shotgun(
    PlayerCharacter owner,
    ISpawnEntity spawnEntity,
    IEntityFinder entityFinder,
    IAudioPlayer audio,
    BulletPool pool,
    CritCalculator critCalculator)
    : GunBase(owner.WeaponBelt.Stats)
{
    private readonly static KnockbackOnHit KnockbackOnHit = new(0.15f);

    protected override TimeSpan Cooldown { get; } = TimeSpan.FromSeconds(1);
    protected override void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        const float bulletSpeed = 1f;
        var baseDamage = 1f * Stats.DamageMultiplier;
        var damage = critCalculator.CalculateCrit(baseDamage, Stats);
        var range = 300f * Stats.RangeMultiplier;

        var targetDirection = target.Position - owner.Position;
        foreach (var direction in ArcSpreader.Arc(targetDirection, 5, MathF.PI / 6))
        {
            var velocity = direction * bulletSpeed * owner.WeaponBelt.Stats.ProjectileSpeedMultiplier;
            var bullet = pool.Get(owner, owner.Position, velocity, damage, range, Stats.Pierce,
                [KnockbackOnHit, ..owner.WeaponBelt.OnHitEffects]);
            spawnEntity.Spawn(bullet);
        }

        audio.Play(SoundEffectTypes.ShotgunShoot);
    }
}