using System;
using System.Linq;

namespace Gameplay.Levelling.SphereGrid.Generation;

/// <summary>
///     Axial hex coordinates (Q,R) with derived S = -Q-R.
///     Q axis is fixed in the TopLeft/BottomRight direction.
///     R axis is fixed in the MiddleLeft/MiddleRight direction.
///     S axis is fixed in the BottomLeft/TopRight direction.
///     Used for hex-grid positioning, distance calculation, and pattern placement.
/// </summary>
public record HexCoordinate(int Q, int R)
{
    /// <summary>
    ///     Derived cube coordinate
    /// </summary>
    public int S => -Q - R;

    /// <summary>
    ///     Returns the neighbour in the given hex direction.
    ///     Directions correspond to EdgeDirection.
    /// </summary>
    public HexCoordinate Neighbour(EdgeDirection direction)
        => direction switch
        {
            EdgeDirection.TopLeft => new HexCoordinate(Q, R - 1),
            EdgeDirection.TopRight => new HexCoordinate(Q + 1, R - 1),
            EdgeDirection.MiddleLeft => new HexCoordinate(Q - 1, R),
            EdgeDirection.MiddleRight => new HexCoordinate(Q + 1, R),
            EdgeDirection.BottomLeft => new HexCoordinate(Q - 1, R + 1),
            EdgeDirection.BottomRight => new HexCoordinate(Q, R + 1),

            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

    /// <summary>
    ///     Returns distance to another hex in steps
    /// </summary>
    public int DistanceTo(HexCoordinate other)
        => (Math.Abs(Q - other.Q) + Math.Abs(R - other.R) + Math.Abs(S - other.S)) / 2;

    /// <summary>
    ///     Rotate 60° clockwise around the origin
    /// </summary>
    public HexCoordinate Rotate60ClockWise()
        => new(-S, -Q);

    /// <summary>
    ///     Rotate 60° counter-clockwise around the origin
    /// </summary>
    public HexCoordinate Rotate60AntiClockWise()
        => new(-R, -S);

    public HexCoordinate Translate(HexCoordinate offset) => new(Q + offset.Q, R + offset.R);

    public Vector2 ToWorldSpace(float hexRadius)
        => new(
            hexRadius * (3f / 2f * Q),
            hexRadius * (MathF.Sqrt(3) * (R + Q / 2f))
        );

    /// <summary>
    ///     Determines the EdgeDirection from one hex to an adjacent hex.
    ///     Assumes the hexes are neighbours.
    /// </summary>
    internal EdgeDirection DirectionTo(HexCoordinate to)
    {
        try
        {
            return Enum.GetValues<EdgeDirection>()
                .First(d => Neighbour(d) == to);
        }
        catch
        {
            throw new InvalidOperationException($"Hexes {this} -> {to} are not adjacent");
        }
    }
}