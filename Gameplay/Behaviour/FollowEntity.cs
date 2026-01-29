using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Behaviour;

public class FollowEntity(IHasPosition target, float speed) : IEnemyMovement
{
    public Vector2 GetIntentVelocity(EnemyBase owner)
    {
        var ownerPosition = owner.Position;

        var directionToTarget = target.Position - ownerPosition;
        if (directionToTarget.LengthSquared() < 0.0001f)
            return Vector2.Zero;

        var normalizedTargetDirection = (Vector2)new UnitVector2(directionToTarget);
        var velocity = normalizedTargetDirection * speed;

        const float scaleFactor = 5f;
        return velocity + owner.SeparationForce * scaleFactor * speed;
    }
}