using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public interface IOnHitEffect
{
    void Apply(Bullet bullet, EnemyBase enemy);
}