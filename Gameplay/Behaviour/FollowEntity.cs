using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Behaviour;

internal class FollowEntity(EnemyBase owner, IHasPosition target, float speed)
{
    internal Vector2 CalculateVelocity(IEnumerable<EnemyBase>? nearbyEnemies = null)
    {
        var direction = target.Position - owner.Position;
        var velocity = (Vector2)new UnitVector2(direction) * speed;

        if (nearbyEnemies == null)
            return velocity;

        // Simple separation from nearby enemies
        var separationForce = Vector2.Zero;

        foreach (var other in nearbyEnemies.Where(e => e != owner))
        {
            var offset = owner.Position - other.Position;
            var distSq = offset.LengthSquared();

            var collisionDistance = (ApproximateRadius(owner) + ApproximateRadius(other)) * 1.2f; // Aim for a small gap
            if (distSq < collisionDistance * collisionDistance)
                separationForce += offset / distSq; // Stronger push when closer
        }

        const float scaleFactor = 40f;
        return velocity + separationForce * scaleFactor * speed;
    }

    private static float ApproximateRadius(EnemyBase enemy) => enemy.Collider.ApproximateRadius;
}