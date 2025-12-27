using Gameplay.Behaviour;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public sealed class SlowOnHit(MovementSlowdown slowdown) : IOnHitEffect
{
    public int Priority => 10;

    public void Apply(IHitContext context) => context.Enemy.ApplySlowdown(slowdown, context.GameTime);
}