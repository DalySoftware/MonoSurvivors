using System;

namespace Gameplay.Combat.Weapons.Projectile;

public abstract class GunBase(WeaponBeltStats stats) : IWeapon
{
    private readonly ExtraShotHandler _extraShotHandler = new(stats);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    protected abstract TimeSpan Cooldown { get; }
    protected WeaponBeltStats Stats => stats;

    public void Update(GameTime gameTime)
    {
        // Handle extra shots first
        var extraShotsResult = _extraShotHandler.Update(gameTime, Shoot);
        if (extraShotsResult is ExtraShotResult.WaitingToFire or ExtraShotResult.Fired)
            return;

        // Normal shooting
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        Shoot();
        _extraShotHandler.QueueFire();
        _remainingCooldown = Cooldown / stats.AttackSpeedMultiplier;
    }
    protected abstract void Shoot();
}