using System;

namespace Gameplay.Levelling.SphereGrid;

public enum EdgeDirection
{
    TopLeft,
    TopRight,
    MiddleLeft,
    MiddleRight,
    BottomLeft,
    BottomRight
}

public static class EdgeDirectionExtensions
{
    public static EdgeDirection Opposite(this EdgeDirection direction) => direction switch
    {
        EdgeDirection.TopLeft => EdgeDirection.BottomRight,
        EdgeDirection.TopRight => EdgeDirection.BottomLeft,
        EdgeDirection.MiddleLeft => EdgeDirection.MiddleRight,
        EdgeDirection.MiddleRight => EdgeDirection.MiddleLeft,
        EdgeDirection.BottomLeft => EdgeDirection.TopRight,
        EdgeDirection.BottomRight => EdgeDirection.TopLeft,
        _ => throw new ArgumentOutOfRangeException(nameof(direction))
    };
}