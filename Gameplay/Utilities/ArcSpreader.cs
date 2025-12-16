using System;
using System.Collections.Generic;

namespace Gameplay.Utilities;

internal static class ArcSpreader
{
    /// <summary>
    ///     Arc mode: first and last bullets sit exactly at the edges of the arc.
    /// </summary>
    internal static IEnumerable<UnitVector2> Arc(Vector2 baseDirection, int count, float totalAngle)
    {
        if (count <= 0) yield break;

        var dir = Vector2.Normalize(baseDirection);

        if (count == 1)
        {
            yield return new UnitVector2(dir);
            yield break;
        }

        var step = totalAngle / (count - 1);
        var start = -totalAngle * 0.5f;

        for (var i = 0; i < count; i++)
        {
            var angle = start + step * i;
            yield return new UnitVector2(Rotate(dir, angle));
        }
    }

    /// <summary>
    ///     Even-spacing mode: bullets are evenly spaced around the centre,
    ///     first/last are inset by half a step.
    /// </summary>
    internal static IEnumerable<UnitVector2> EvenlySpace(Vector2 baseDirection, int count, float totalAngle)
    {
        if (count <= 0) yield break;

        var dir = Vector2.Normalize(baseDirection);

        if (count == 1)
        {
            yield return new UnitVector2(dir);
            yield break;
        }

        var step = totalAngle / count;
        var start = -totalAngle * 0.5f + step * 0.5f; // shift by half step

        for (var i = 0; i < count; i++)
        {
            var angle = start + step * i;
            yield return new UnitVector2(Rotate(dir, angle));
        }
    }
    internal static Vector2 RandomDirection()
    {
        var angle = (float)(Random.Shared.NextDouble() * MathF.Tau);
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
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