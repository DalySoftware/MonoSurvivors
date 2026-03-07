using System;
using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Behaviour;

public sealed class SlitherFollowEntity(
    IHasPosition target,
    float speed,
    float slitherAmount,
    float slitherFrequencyHz)
    : IEnemyMovement
{
    public Vector2 GetIntentVelocity(EnemyBase owner, GameTime gameTime)
    {
        var toTarget = target.Position - owner.Position;
        if (toTarget.LengthSquared() < 0.0001f)
            return Vector2.Zero;

        var forward = (Vector2)new UnitVector2(toTarget);
        var lateral = new Vector2(-forward.Y, forward.X);

        var timeSeconds = (float)gameTime.TotalGameTime.TotalSeconds;
        var slither = MathF.Sin(timeSeconds * MathF.Tau * slitherFrequencyHz);

        var direction = forward + lateral * (slither * slitherAmount);
        if (direction.LengthSquared() < 0.0001f)
            direction = forward;
        else
            direction.Normalize();

        const float separationScale = 5f;
        return direction * speed + owner.SeparationForce * separationScale * speed;
    }
}