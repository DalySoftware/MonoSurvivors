namespace Gameplay.Combat.Weapons.OnHitEffects;

public sealed class KnockbackOnHit(float strength) : IOnHitEffect
{
    public int Priority => 10;

    public void Apply(IHitContext context)
    {
        if (context is not BulletHitContext bulletContext)
            return;

        var enemy = context.Enemy;

        var direction = enemy.Position - bulletContext.Bullet.Position;
        if (direction.LengthSquared() <= 1f) // Avoid chaotic behaviour if their centres are within a pixel  
            return;

        direction = Vector2.Normalize(direction);

        enemy.ApplyKnockback(direction * strength);
    }
}