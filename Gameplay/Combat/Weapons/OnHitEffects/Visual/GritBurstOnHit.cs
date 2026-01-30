using Gameplay.Entities;
using Gameplay.Entities.Effects;

namespace Gameplay.Combat.Weapons.OnHitEffects.Visual;

public sealed class GritBurstOnHit(ISpawnEntity spawnEntity, GritBurstPool pool) : IOnHitVisualEffect
{
    public void Apply(IHitContext hitContext)
    {
        if (hitContext is not BulletHitContext { Bullet: var bullet, Enemy: var enemy })
            return;

        var velocity = bullet.Velocity;
        if (velocity.LengthSquared() < 0.00001f)
            return;

        // Spawn at bullet position. May not be super accurate for fast bullets
        var burst = pool.Get(bullet.Position, velocity, enemy.Velocity, enemy.GritColor);

        spawnEntity.Spawn(burst);
    }
}