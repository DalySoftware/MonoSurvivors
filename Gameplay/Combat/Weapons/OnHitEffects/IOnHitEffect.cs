using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public interface IOnHitEffect
{
    int Priority { get; }
    void Apply(IHitContext context);
}

public interface IHitContext
{
    PlayerCharacter Owner { get; }
    EnemyBase Enemy { get; }
}

public sealed record HitContext(PlayerCharacter Owner, EnemyBase Enemy) : IHitContext;

public sealed class BulletHitContext(PlayerCharacter owner, EnemyBase enemy, Bullet bullet) : IHitContext
{
    public Bullet Bullet { get; } = bullet;

    internal bool ConsumePierce { get; private set; } = true;
    internal bool Bounce { get; private set; }
    internal bool Delete { get; private set; } = false;

    internal Vector2? BounceVelocity { get; private set; }
    internal float BounceDamageMultiplier { get; private set; } = 1f;
    public PlayerCharacter Owner { get; } = owner;
    public EnemyBase Enemy { get; } = enemy;

    public void RequestBounce(Vector2 newVelocity, float damageMultiplier = 1f)
    {
        Bounce = true;
        ConsumePierce = false;
        BounceVelocity = newVelocity;
        BounceDamageMultiplier = damageMultiplier;
    }
}