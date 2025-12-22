using System;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public class BounceOnHit : IOnHitEffect
{
    public int Priority => 1;

    public void Apply(IHitContext context)
    {
        if (context is not BulletHitContext bulletContext)
            return;

        var bullet = bulletContext.Bullet;
        var enemy = bulletContext.Enemy;

        // Reflection
        var normal = Vector2.Normalize(bullet.Position - enemy.Position);
        var velocity = bullet.Velocity - 2 * Vector2.Dot(bullet.Velocity, normal) * normal;

        // Add a small random rotation
        const float maxAngleOffset = MathF.PI / 4;
        var angleOffset = (Random.Shared.NextSingle() * 2f - 1f) * maxAngleOffset; // [-max, +max]

        velocity = Vector2.Transform(velocity, Matrix.CreateRotationZ(angleOffset));

        bulletContext.RequestBounce(velocity, 0.6f);
    }
}