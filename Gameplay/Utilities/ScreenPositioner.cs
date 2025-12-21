using System;
using Microsoft.Xna.Framework.Graphics;

namespace Gameplay.Utilities;

public class ScreenPositioner(GraphicsDevice graphics, float buffer)
{
    /// <summary>
    ///     Get a random position off-screen from the player.
    /// </summary>
    /// <param name="centre">Centre-point to spawn around.</param>
    /// <param name="buffer">
    ///     Extra buffer to ensure it's off-screen.
    ///     <example>0.2f would spawn around 20% off-screen</example>
    /// </param>
    /// <returns>World-space <see cref="Vector2" /> position</returns>
    internal Vector2 GetRandomOffScreenPosition(Vector2 centre)
    {
        // Random angle
        var angle = (float)Random.Shared.NextDouble() * 2f * MathF.PI;

        // Calculate distance based on angle to account for rectangular viewport
        var halfWidth = graphics.Viewport.Width * 0.5f;
        var halfHeight = graphics.Viewport.Height * 0.5f;
        var cos = MathF.Abs(MathF.Cos(angle));
        var sin = MathF.Abs(MathF.Sin(angle));

        // Distance to edge of rectangle at this angle, plus a small buffer
        // The buffer makes sure it's slightly offscreen, unless the player outruns camera by a crazy speed
        var distanceFromPlayer = MathF.Min(halfWidth / cos, halfHeight / sin) * (1f + buffer);

        var x = centre.X + MathF.Cos(angle) * distanceFromPlayer;
        var y = centre.Y + MathF.Sin(angle) * distanceFromPlayer;

        return new Vector2(x, y);
    }
}