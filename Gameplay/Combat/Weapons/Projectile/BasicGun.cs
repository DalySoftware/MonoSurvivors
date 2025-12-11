using System;
using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : IWeapon
{
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);

    private readonly TimeSpan _extraShotCooldown = TimeSpan.FromSeconds(0.2);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    private TimeSpan _remainingExtraShotCooldown = TimeSpan.Zero;
    private int _remainingExtraShots = 0;

    public void Update(GameTime gameTime, WeaponBeltStats stats)
    {
        // Handle extra shots first
        if (_remainingExtraShots > 0)
        {
            _remainingExtraShotCooldown -= gameTime.ElapsedGameTime;
            if (_remainingExtraShotCooldown > TimeSpan.Zero) return;

            Shoot(stats);
            _remainingExtraShots--;
            _remainingExtraShotCooldown = _extraShotCooldown / stats.AttackSpeedMultiplier;
            return;
        }

        // Normal shooting
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        Shoot(stats);
        _remainingCooldown = _cooldown / stats.AttackSpeedMultiplier;

        // Queue extra shots
        _remainingExtraShots = stats.ExtraShots;
        _remainingExtraShotCooldown = _extraShotCooldown;
    }

    private void Shoot(WeaponBeltStats stats)
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        var baseDamage = 8f * stats.DamageMultiplier;
        var damage = CritCalculator.CalculateDamage(baseDamage, stats.CritChance, stats.CritDamage);
        var range = 300f * stats.RangeMultiplier;
        var bullet = new Bullet(owner.Position, target.Position, damage, range, stats.Pierce,
            1f * stats.SpeedMultiplier);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
}