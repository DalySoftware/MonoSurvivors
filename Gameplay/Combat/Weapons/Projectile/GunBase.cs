using System;

namespace Gameplay.Combat.Weapons.Projectile;

public abstract class GunBase : IWeapon
{
    private readonly ExtraShotHandler _extraShotHandler;
    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    protected GunBase(WeaponBeltStats stats)
    {
        Stats = stats;
        _extraShotHandler = new ExtraShotHandler(stats, Shoot);
    }

    protected abstract TimeSpan Cooldown { get; }
    protected WeaponBeltStats Stats { get; }

    public void Update(GameTime gameTime)
    {
        // Handle extra shots first
        var extraShotsResult = _extraShotHandler.Update(gameTime);
        if (extraShotsResult is ExtraShotResult.WaitingToFire or ExtraShotResult.Fired)
            return;

        // Normal shooting
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        Shoot();
        _extraShotHandler.QueueFire();
        _remainingCooldown = Cooldown / Stats.AttackSpeedMultiplier;
    }

    protected abstract void Shoot();
}