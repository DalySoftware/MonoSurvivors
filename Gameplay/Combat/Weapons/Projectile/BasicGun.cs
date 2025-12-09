using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Audio;
using Gameplay.Entities;
using Gameplay.Levelling.PowerUps.Weapon;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : IWeapon
{
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    
    private readonly TimeSpan _extraShotCooldown = TimeSpan.FromSeconds(0.2);
    private TimeSpan _remainingExtraShotCooldown = TimeSpan.Zero;
    private int _remainingExtraShots = 0;

    public void Update(GameTime gameTime, IReadOnlyCollection<IWeaponPowerUp> powerUps)
    {
        var attackSpeedMultiplier = AttackSpeedMultiplier(powerUps);
        var damageMultiplier = powerUps.OfType<DamageUp>().Sum(p => p.Value) + 1f;
        var rangeMultiplier = powerUps.OfType<RangeUp>().Sum(p => p.Value) + 1f;
        
        // Handle extra shots first
        if (_remainingExtraShots > 0)
        {
            _remainingExtraShotCooldown -= gameTime.ElapsedGameTime;
            if (_remainingExtraShotCooldown > TimeSpan.Zero) return;
            
            Shoot(damageMultiplier, rangeMultiplier);
            _remainingExtraShots--;
            _remainingExtraShotCooldown = _extraShotCooldown / attackSpeedMultiplier;
            return;
        }

        // Normal shooting
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        Shoot(damageMultiplier,  rangeMultiplier);
        _remainingCooldown = _cooldown / attackSpeedMultiplier;

        // Queue extra shots
        _remainingExtraShots = ExtraShots(powerUps);
        _remainingExtraShotCooldown = _extraShotCooldown;
    }

    private void Shoot(float damageMultiplier = 1f, float  rangeMultiplier = 1f)
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        var damage = 10f * damageMultiplier;
        var range = 200f * rangeMultiplier;
        var bullet = new Bullet(owner.Position, target.Position, damage, range);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
    
    private float AttackSpeedMultiplier(IReadOnlyCollection<IWeaponPowerUp> powerUps) =>
        powerUps.OfType<AttackSpeedUp>().Sum(p => p.Value) + 1f;
    
    private int ExtraShots(IReadOnlyCollection<IWeaponPowerUp> powerUps) =>
        powerUps.OfType<ShotCountUp>().Sum(p => p.ExtraShots);
}