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

public sealed record BulletHitContext(PlayerCharacter Owner, EnemyBase Enemy, Bullet Bullet) : IHitContext;