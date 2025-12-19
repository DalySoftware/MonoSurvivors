using System;
using Gameplay.Entities;

namespace Gameplay.Behaviour;

public class HealthRegenManager
{
    private readonly TimeSpan _baseCooldown = TimeSpan.FromMinutes(1.5);
    private TimeSpan _remainingCooldown = TimeSpan.Zero;

    internal void Update(GameTime gameTime, PlayerCharacter player)
    {
        _remainingCooldown -= gameTime.ElapsedGameTime;
        if (_remainingCooldown > TimeSpan.Zero) return;

        _remainingCooldown = _baseCooldown / MathF.Max(player.HealthRegen, 1f);
    }
}