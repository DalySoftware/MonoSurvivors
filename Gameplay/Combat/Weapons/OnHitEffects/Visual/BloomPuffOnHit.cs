using System;
using Gameplay.Entities;
using Gameplay.Entities.Effects;
using Gameplay.Rendering.Effects.SpriteBatch;

namespace Gameplay.Combat.Weapons.OnHitEffects.Visual;

public sealed class BloomPuffOnHit(EntityManager entityManager, BloomPuffPool pool, EffectManager effectManager)
    : IOnHitVisualEffect
{
    private readonly static TimeSpan Duration = TimeSpan.FromMilliseconds(120);

    public void Apply(IHitContext context)
    {
        if (context is not BulletHitContext hitContext) return;

        const int startRadius = 18;
        const int endRadius = 36;

        var puff = pool.Get(hitContext.Bullet.Position, hitContext.EffectColor, startRadius, endRadius, Duration);

        entityManager.Spawn(puff);
        effectManager.FireEffect(puff, SpriteBatchEffect.Additive, context.GameTime, Duration); // Same duration
    }
}