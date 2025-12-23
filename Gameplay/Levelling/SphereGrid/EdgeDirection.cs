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
        public EdgeDirection RotateClockwise(int steps)
        {
            // Normalise steps so negatives and large values work
            var normalized = (steps % Count + Count) % Count;
            return (EdgeDirection)(((int)direction + normalized) % Count);
        }
    }
}