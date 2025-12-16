using System;
using Gameplay.Audio;
using Gameplay.Entities;

namespace Gameplay.Combat.Weapons.Projectile;

public class BasicGun(PlayerCharacter owner, ISpawnEntity spawnEntity, IEntityFinder entityFinder, IAudioPlayer audio)
    : IWeapon
{
    private const float BulletSpeed = 1f;
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);

    private readonly TimeSpan _extraShotCooldown = TimeSpan.FromSeconds(0.2);
    private readonly BulletSplitter _bulletSplitter = new(owner, MathF.PI / 6, 0.5f, spawnEntity);

    private WeaponBeltStats _stats = new();

    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    private TimeSpan _remainingExtraShotCooldown = TimeSpan.Zero;
    private int _remainingExtraShots = 0;

    public void Update(GameTime gameTime, WeaponBeltStats stats)
    {
        _stats = stats;

        // Handle extra shots first
        if (FireExtraShots(gameTime) is ExtraShotsResult.WaitingToFire or ExtraShotsResult.Fired) return;

        // Normal shooting
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        Shoot();
        _remainingCooldown = _cooldown / stats.AttackSpeedMultiplier;

        // Queue extra shots
        _remainingExtraShots = stats.ExtraShots;
        _remainingExtraShotCooldown = _extraShotCooldown;
    }

    private ExtraShotsResult FireExtraShots(GameTime gameTime)
    {
        if (_remainingExtraShots <= 0) return ExtraShotsResult.NotFiring;

        _remainingExtraShotCooldown -= gameTime.ElapsedGameTime;
        if (_remainingExtraShotCooldown > TimeSpan.Zero) return ExtraShotsResult.WaitingToFire;

        Shoot();
        _remainingExtraShots--;
        _remainingExtraShotCooldown = _extraShotCooldown / _stats.AttackSpeedMultiplier;
        return ExtraShotsResult.Fired;
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

    private enum ExtraShotsResult
    {
        NotFiring,
        WaitingToFire,
        Fired,
    }
}