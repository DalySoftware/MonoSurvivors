using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Behaviour;

internal class FollowEntity(EnemyBase owner, IHasPosition target, float speed)
{
    internal Vector2 CalculateVelocity(Vector2 separationForce)
    {
        var ownerPosition = owner.Position;

        var directionToTarget = target.Position - ownerPosition;
        if (directionToTarget.LengthSquared() < 0.0001f)
            return Vector2.Zero;

        var normalizedTargetDirection = (Vector2)new UnitVector2(directionToTarget);
        var velocity = normalizedTargetDirection * speed;

        const float scaleFactor = 5f;
        return velocity + separationForce * scaleFactor * speed;
    }
}