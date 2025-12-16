namespace Gameplay.Levelling.SphereGrid;

public enum EdgeDirection
{
    TopLeft,
    TopRight,
    MiddleRight,
    BottomRight,
    BottomLeft,
    MiddleLeft,
}

public static class EdgeDirectionExtensions
{
    private const int Count = 6;

    extension(EdgeDirection direction)
    {
        public EdgeDirection Opposite() => (EdgeDirection)(((int)direction + Count / 2) % Count);
        public EdgeDirection RotateClockwiseOnce() => (EdgeDirection)(((int)direction + 1) % Count);
        public EdgeDirection RotateAntiClockwiseOnce() => (EdgeDirection)(((int)direction + Count - 1) % Count);
    }
}