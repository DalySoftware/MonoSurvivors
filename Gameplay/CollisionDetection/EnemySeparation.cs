using System;
using System.Collections.Generic;
using Gameplay.Entities.Enemies;

namespace Gameplay.CollisionDetection;

public sealed class EnemySeparation(SpatialHashManager spatialHashManager)
{
    private const int SeparationDivisor = 3; // update ~1/3 enemies per frame
    private const float Smoothing = 0.25f;
    private const float DesiredGapMultiplier = 1.2f;
    private const int MaximumContributors = 8;
    private const float MaxForce = 2.0f;

    private readonly List<EnemyBase> _nearbyScratch = new(64);
    private int _cursor;

    internal void Update(IReadOnlyList<EnemyBase> enemies)
    {
        var count = enemies.Count;
        if (count == 0)
            return;

        var batch = (count + SeparationDivisor - 1) / SeparationDivisor;

        for (var k = 0; k < batch; k++)
        {
            var i = (_cursor + k) % count;
            var enemy = enemies[i];

            var newForce = Compute(enemy);
            enemy.SeparationForce = Vector2.Lerp(enemy.SeparationForce, newForce, Smoothing);
        }

        _cursor = (_cursor + batch) % count;
    }

    private Vector2 Compute(EnemyBase owner)
    {
        var ownerPos = owner.Position;
        var ownerRadius = owner.ApproximateRadius;

        var contributors = 0;
        var separation = Vector2.Zero;

        spatialHashManager.EnemyNeighborhood.QueryNearbyInto(ownerPos, _nearbyScratch); // cellRadius default = 1

        for (var i = 0; i < _nearbyScratch.Count; i++)
        {
            var other = _nearbyScratch[i];
            if (other == owner)
                continue;

            var offset = ownerPos - other.Position;
            var distSq = offset.LengthSquared();

            var collisionDistance = (ownerRadius + other.ApproximateRadius) * DesiredGapMultiplier;
            var collisionDistSq = collisionDistance * collisionDistance;

            if (distSq >= collisionDistSq)
                continue;

            // Bounded, stable repulsion (depth-based)
            var dist = MathF.Sqrt(distSq);
            var depth = collisionDistance - dist;
            var n = offset / (dist + 0.0001f);
            separation += n * (depth / collisionDistance);

            contributors++;
            if (contributors >= MaximumContributors)
                break;
        }

        // Clamp
        var lenSq = separation.LengthSquared();
        if (lenSq > MaxForce * MaxForce)
            separation *= MaxForce / MathF.Sqrt(lenSq);

        return separation;
    }
}