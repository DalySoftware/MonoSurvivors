using System.Collections.Generic;
using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Behaviour;

internal class FollowEntity(EnemyBase owner, IHasPosition target, float speed)
{
    internal Vector2 CalculateVelocity(List<EnemyBase> nearbyEnemies)
    {
        var ownerPosition = owner.Position;

        var directionToTarget = target.Position - ownerPosition;
        if (directionToTarget.LengthSquared() < 0.0001f)
            return Vector2.Zero;

        var normalizedTargetDirection = (Vector2)new UnitVector2(directionToTarget);
        var velocity = normalizedTargetDirection * speed;

        var ownerRadius = ApproximateRadius(owner);
        const float desiredGapMultiplier = 1.2f; // Keep ~20% gap between
        const int maximumSeparationContributors = 8;

        var contributingNeighbourCount = 0;
        var separationForce = Vector2.Zero;

        foreach (var other in nearbyEnemies)
        {
            if (other == owner)
                continue;

            var offsetFromOther = ownerPosition - other.Position;
            var distanceSquared = offsetFromOther.LengthSquared();

            var collisionDistance = (ownerRadius + ApproximateRadius(other)) * desiredGapMultiplier;
            var collisionDistanceSquared = collisionDistance * collisionDistance;

            if (distanceSquared >= collisionDistanceSquared)
                continue;

            var inverseDistance = 1f / (distanceSquared + 0.0001f);
            separationForce += offsetFromOther * inverseDistance;

            contributingNeighbourCount++;
            if (contributingNeighbourCount >= maximumSeparationContributors)
                break;
        }

        const float scaleFactor = 40f;
        return velocity + separationForce * scaleFactor * speed;
    }

    private static float ApproximateRadius(EnemyBase enemy)
    {
        // Fast path when Colliders is actually an array (which your new profile suggests)
        var max = 0f;
        for (var i = 0; i < enemy.Colliders.Length; i++)
        {
            var r = enemy.Colliders[i].ApproximateRadius;
            if (r > max) max = r;
        }

        return max;
    }
}