using System;

namespace Gameplay.Combat.Weapons;

internal class ExtraShotHandler(WeaponBeltStats stats)
{
    private int _remainingShots;
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    private TimeSpan Cooldown => TimeSpan.FromSeconds(0.2) / stats.AttackSpeedMultiplier;

    internal ExtraShotResult Update(GameTime gameTime, Action shoot)
    {
        if (_remainingShots <= 0) return ExtraShotResult.NotFiring;

        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return ExtraShotResult.WaitingToFire;

        shoot();
        _remainingShots--;
        _remainingCooldown = Cooldown;
        return ExtraShotResult.Fired;
    }

    internal void QueueFire() => _remainingShots = stats.ExtraShots;
}

internal enum ExtraShotResult
{
    NotFiring,
    WaitingToFire,
    Fired,
}