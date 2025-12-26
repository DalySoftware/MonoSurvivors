using System.Collections.Generic;
using Gameplay.Combat.Weapons.OnHitEffects;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities.Enemies;
using Gameplay.Utilities;

namespace Gameplay.Entities.Pooling;

public class BulletPool
{
    private readonly Stack<Bullet> _pool = new();

    public Bullet Get(
        PlayerCharacter owner,
        Vector2 position,
        Vector2 target,
        float speed,
        float damage,
        float range,
        int pierce = 0,
        IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null)
    {
        var velocity = VectorCalculations.Velocity(position, target, speed);
        return Get(owner, position, velocity, damage, range, pierce, onHits, immuneEnemies);
    }

    public Bullet Get(
        PlayerCharacter owner,
        Vector2 position,
        Vector2 velocity,
        float damage,
        float range,
        int pierce = 0,
        IEnumerable<IOnHitEffect>? onHits = null,
        HashSet<EnemyBase>? immuneEnemies = null)
    {
        if (_pool.TryPop(out var bullet))
            return bullet.Reinitialize(owner, position, velocity, damage, range, pierce, onHits, immuneEnemies);

        return new Bullet(this, owner, position, velocity, damage, range, pierce, onHits, immuneEnemies);
    }

    internal void Return(Bullet bullet) => _pool.Push(bullet);
}