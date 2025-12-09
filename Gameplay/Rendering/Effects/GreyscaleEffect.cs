using System;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Greyscale/desaturation effect. Renders the entity in greyscale for the duration.
/// </summary>
public sealed class GreyscaleEffect(TimeSpan duration) : VisualEffect(duration)
{
    public override Color ComputeColor() => Color.DarkGray;
}