using System;
using System.Collections.Generic;

namespace Gameplay.Utilities;

internal static class VectorSpreader
{
    /// <summary>
    ///     Returns evenly spaced direction vectors centered around the base vector.
    /// </summary>
    internal static IEnumerable<Vector2> EvenSpread(
        Vector2 baseDirection,
        int count,
        float totalAngle)
    {
        if (count <= 0)
            yield break;

        if (count == 1)
        {
            yield return Vector2.Normalize(baseDirection);
            yield break;
        }

        var dir = Vector2.Normalize(baseDirection);
        var step = totalAngle / (count - 1);
        var start = -totalAngle * 0.5f;

        for (var i = 0; i < count; i++)
        {
            var angle = start + step * i;
            yield return Rotate(dir, angle);
        }
    }

    private static Vector2 Rotate(Vector2 v, float radians)
    {
        var sin = MathF.Sin(radians);
        var cos = MathF.Cos(radians);

        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos);
    }
}