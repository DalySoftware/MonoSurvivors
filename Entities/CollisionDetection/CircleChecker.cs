using Entities.Combat;

namespace Entities.CollisionDetection;

internal static class CircleChecker
{
    internal static bool HasOverlap(IDamagesPlayer source, IDamageablePlayer target)
    {
        var sourceRadius = source.CollisionRadius;
        var targetRadius = target.CollisionRadius;

        var distanceSquared = Vector2.DistanceSquared(source.Position, target.Position);
        return distanceSquared <= (sourceRadius + targetRadius) * (sourceRadius + targetRadius);
    }

    internal static bool HasOverlap(IDamagesEnemies source, IDamageableEnemy target)
    {
        var sourceRadius = source.CollisionRadius;
        var targetRadius = target.CollisionRadius;

        var distanceSquared = Vector2.DistanceSquared(source.Position, target.Position);
        return distanceSquared <= (sourceRadius + targetRadius) * (sourceRadius + targetRadius);
    }
}