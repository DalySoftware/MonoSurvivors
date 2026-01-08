using System;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Base type for visual effects. Tracks duration
/// </summary>
public abstract class VisualEffect(TimeSpan duration)
{
    private TimeSpan _remainingDuration = duration;

    public bool IsActive => _remainingDuration > TimeSpan.Zero;

    public void Update(GameTime gameTime) => _remainingDuration -= gameTime.ElapsedGameTime;
}