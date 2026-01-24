using System;
using System.Collections.Generic;
using Gameplay.Entities.Enemies;

namespace Gameplay.CollisionDetection;

internal static class EnemySeparation
{
    private readonly static List<EnemyBase> NearbyScratch = new(64);

    internal static Vector2 Compute(EnemyBase owner, SpatialPointHash<EnemyBase> neighborhood)
    {
        var ownerPos = owner.Position;
        var ownerRadius = owner.ApproximateRadius;

        const float desiredGapMultiplier = 1.2f;
        const int maximumContributors = 8;

        var contributors = 0;
        var separation = Vector2.Zero;

        // cellRadius=1 assumes neighborhood.CellSize matches your avoidance scale.
        neighborhood.QueryNearbyInto(ownerPos, NearbyScratch);

        for (var i = 0; i < NearbyScratch.Count; i++)
        {
            var other = NearbyScratch[i];
            if (other == owner)
                continue;

            var offset = ownerPos - other.Position;
            var distSq = offset.LengthSquared();

            var collisionDistance = (ownerRadius + other.ApproximateRadius) * desiredGapMultiplier;
            var collisionDistSq = collisionDistance * collisionDistance;

            if (distSq >= collisionDistSq)
                continue;

            // Bounded, stable repulsion
            var dist = MathF.Sqrt(distSq);
            var depth = collisionDistance - dist;
            var n = offset / (dist + 0.0001f);
            separation += n * (depth / collisionDistance);

            contributors++;
            if (contributors >= maximumContributors)
                break;
        }

        const float max = 2.0f;
        var lenSq = separation.LengthSquared();
        if (lenSq > max * max)
            separation *= max / MathF.Sqrt(lenSq);

        return separation;
    }
}