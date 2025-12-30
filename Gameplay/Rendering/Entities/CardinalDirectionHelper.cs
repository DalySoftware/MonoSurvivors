using System;

namespace Gameplay.Rendering;

internal static class CardinalDirectionHelper
{
    internal static (int x, int y) ToCardinalDirection(Vector2 direction)
    {
        // Preserve explicit no-direction
        if (direction == Vector2.Zero)
            return (1, 1);

        var unitVector = Vector2.Normalize(direction);

        // Round into the 8-way grid, but allow zeros in either axis
        var x = (int)MathF.Round(unitVector.X);
        var y = (int)MathF.Round(unitVector.Y);

        // Map from direction space [-1, 1] to sheet cell space [0, 2]
        return (x + 1, y + 1);
    }
}