using System;

namespace Gameplay.Combat.Weapons.Projectile;

public abstract class GunBase(WeaponBeltStats stats) : IWeapon
{
    private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(1);
    private readonly ExtraShotHandler _extraShotHandler = new(stats);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    protected WeaponBeltStats Stats { get; private set; } = new();

    public void Update(GameTime gameTime, WeaponBeltStats stats)
    {
        Stats = stats;

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
    protected abstract void Shoot();
}