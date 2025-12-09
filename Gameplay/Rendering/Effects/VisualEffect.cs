using System;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Base type for visual effects. Tracks duration and computes color at each progress point.
/// </summary>
public abstract class VisualEffect(TimeSpan duration)
{
    private readonly TimeSpan _totalDuration = duration;
    private TimeSpan _remainingDuration = duration;

    public bool IsActive => _remainingDuration > TimeSpan.Zero;

    /// <summary>
    ///     Progress from 0 (just started) to 1 (complete)
    /// </summary>
    public float Progress => (float)((_totalDuration - _remainingDuration).TotalSeconds / _totalDuration.TotalSeconds);

    /// <summary>
    ///     Compute the color for this effect at its current progress
    /// </summary>
    public virtual Color ComputeColor() => Color.White;

    public void Update(GameTime gameTime) => _remainingDuration -= gameTime.ElapsedGameTime;
}