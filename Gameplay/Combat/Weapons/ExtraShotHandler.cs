using System;

namespace Gameplay.Combat.Weapons;

internal class ExtraShotHandler(WeaponBeltStats stats, Action shoot)
{
    private int _remainingShots;
    private TimeSpan _remainingCooldown = TimeSpan.Zero;
    private TimeSpan Cooldown => TimeSpan.FromSeconds(0.2) / stats.AttackSpeedMultiplier;

    internal ExtraShotResult Update(GameTime gameTime)
    {
        if (_remainingShots <= 0) return ExtraShotResult.NotFiring;

        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return ExtraShotResult.WaitingToFire;

        shoot();
        _remainingShots--;
        _remainingCooldown = Cooldown;
        return ExtraShotResult.Fired;
    }

    internal void QueueFire()
    {
        var chance = Math.Max(0f, stats.ExtraShotChance);

        var guaranteedShots = (int)MathF.Floor(chance);
        var fractionalChance = chance - guaranteedShots;

        var extraFromFraction = Random.Shared.NextSingle() < fractionalChance ? 1 : 0;

        _remainingShots = guaranteedShots + extraFromFraction;
    }
}

internal enum ExtraShotResult
{
    NotFiring,
    WaitingToFire,
    Fired,
}