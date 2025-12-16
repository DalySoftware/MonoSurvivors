using System;
using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : IWeapon
{
    private const float BulletSpeed = 1f;
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);

    private readonly BulletSplitter _bulletSplitter = new(owner, MathF.PI / 6, 0.5f, spawnEntity);
    private readonly ExtraShotHandler _extraShotHandler = new(owner.WeaponBelt.Stats);

    private WeaponBeltStats _stats = new();

    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    public void Update(GameTime gameTime, WeaponBeltStats stats)
    {
        _stats = stats;

        // Handle extra shots first
        var extraShotsResult = _extraShotHandler.Update(gameTime, Shoot);
        if (extraShotsResult is ExtraShotResult.WaitingToFire or ExtraShotResult.Fired)
            return;

        // Normal shooting
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        Shoot();
        _extraShotHandler.QueueFire();
        _remainingCooldown = _cooldown / stats.AttackSpeedMultiplier;
    }

    private void Shoot()
    {
        var target = entityFinder.NearestEnemyTo(owner);
        if (target == null) return;

        var baseDamage = 8f * _stats.DamageMultiplier;
        var damage = CritCalculator.CalculateDamage(baseDamage, _stats.CritChance, _stats.CritDamage);
        var range = 300f * _stats.RangeMultiplier;
        var bullet = new Bullet(owner, owner.Position, target.Position, damage, range, _stats.Pierce,
            BulletSpeed * _stats.SpeedMultiplier, _bulletSplitter.SpawnAnyOnHitBulletSplits);
        spawnEntity.Spawn(bullet);
        audio.Play(SoundEffectTypes.Shoot);
    }
}