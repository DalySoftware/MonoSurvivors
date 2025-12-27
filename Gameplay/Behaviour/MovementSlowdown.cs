using System;

namespace Gameplay.Behaviour;

public readonly record struct MovementSlowdown
{
    internal MovementSlowdown(float amount, TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(amount, 1f);
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, 0f);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);

        Amount = amount;
        Duration = duration;
    }
    public float Amount { get; }
    public TimeSpan Duration { get; }
}