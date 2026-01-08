using System;
using Gameplay.Rendering.Colors;

namespace Gameplay.Rendering.Effects;

/// <summary>
///     Greyscale/desaturation effect. Renders the entity in greyscale for the duration.
/// </summary>
public sealed class GreyscaleEffect(TimeSpan duration) : VisualEffect(duration)
{
    public Color ComputeColor() => ColorPalette.DarkGray;
}