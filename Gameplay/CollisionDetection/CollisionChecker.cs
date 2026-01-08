using System;

namespace Gameplay.CollisionDetection;

internal static class CollisionChecker
{
    internal static bool HasOverlap(CircleCollider source, CircleCollider target)
    {
        var sourceRadius = source.CollisionRadius;
        var targetRadius = target.CollisionRadius;

        var distanceSquared = Vector2.DistanceSquared(source.Position, target.Position);
        return distanceSquared <= (sourceRadius + targetRadius) * (sourceRadius + targetRadius);
    }

    internal static bool HasOverlap(CircleCollider source, RectangleCollider target)
    {
        // Find the closest point on the rectangle to the circle's centre
        var closestX = Math.Clamp(source.Position.X, target.Left, target.Right);
        var closestY = Math.Clamp(source.Position.Y, target.Top, target.Bottom);

        // Calculate distance from circle's centre to this closest point
        var distanceX = source.Position.X - closestX;
        var distanceY = source.Position.Y - closestY;
        var distanceSquared = distanceX * distanceX + distanceY * distanceY;

        // Collision occurs if distance is less than circle's radius
        return distanceSquared <= source.CollisionRadius * source.CollisionRadius;
    }

    internal static bool HasOverlap(RectangleCollider source, RectangleCollider target) =>
        source.Left < target.Right &&
        source.Right > target.Left &&
        source.Top < target.Bottom &&
        source.Bottom > target.Top;

    internal static bool HasOverlap(ICollider source, ICollider target) => (source, target) switch
    {
        (CircleCollider sourceCircle, CircleCollider targetCircle) => HasOverlap(sourceCircle, targetCircle),
        (CircleCollider circle, RectangleCollider rect) => HasOverlap(circle, rect),
        (RectangleCollider rect, CircleCollider circle) => HasOverlap(circle, rect),
        (RectangleCollider sourceRect, RectangleCollider targetRect) => HasOverlap(sourceRect, targetRect),
        _ => throw new InvalidOperationException("Unsupported collision check"),
    };
}

internal static class ColliderExtensions
{
    extension(RectangleCollider collider)
    {
        internal float Left => (collider.Position.X - collider.Width) * 0.5f;
        internal float Right => (collider.Position.X + collider.Width) * 0.5f;
        internal float Top => (collider.Position.Y - collider.Height) * 0.5f;
        internal float Bottom => (collider.Position.Y + collider.Height) * 0.5f;
    }
}