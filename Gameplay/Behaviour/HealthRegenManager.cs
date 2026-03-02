using System;
using Gameplay.Entities;

namespace Gameplay.Behaviour;

public class HealthRegenManager
{
    private readonly TimeSpan _baseCooldown = TimeSpan.FromMinutes(1.5);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    internal void Update(GameTime gameTime, PlayerCharacter player)
    {
        var regen = player.Stats.HealthRegen;
        if (regen <= 0) return;

        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = _baseCooldown / MathF.Max(regen, 1f);
        player.Heal(1);
    }
}