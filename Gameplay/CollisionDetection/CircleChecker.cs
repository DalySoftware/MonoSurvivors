namespace Gameplay.CollisionDetection;

internal static class CircleChecker
{
    internal static bool HasOverlap(ICircleCollider source, ICircleCollider target)
    {
        var sourceRadius = source.CollisionRadius;
        var targetRadius = target.CollisionRadius;

        var distanceSquared = Vector2.DistanceSquared(source.Position, target.Position);
        return distanceSquared <= (sourceRadius + targetRadius) * (sourceRadius + targetRadius);
    }
}