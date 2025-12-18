using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Combat.Weapons.Projectile;
using Gameplay.Entities;
using Gameplay.Entities.Effects;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat.Weapons.OnHitEffects;

public sealed class ChainLightningOnHit(
    IEntityFinder entityFinder,
    ISpawnEntity spawnEntity)
    : IOnHitEffect
{
    private const float DamageFalloff = 0.6f;
    private const float MaxJumpDistance = 200f;

    private const int MaxChains = 3;
    private const float BaseDamage = 8f;

    public void Apply(
        Bullet bullet,
        EnemyBase enemy)
    {
        var owner = bullet.Owner;
        var stats = owner.WeaponBelt.Stats;

        var chance = MathF.Min(stats.ChainLightningChance, 1f);
        if (Random.Shared.NextSingle() > chance)
            return;

        var remainingChains = MaxChains;

        var hitEnemies = new HashSet<EnemyBase> { enemy };

        var currentEnemy = enemy;
        var damage = BaseDamage * stats.DamageMultiplier;

        while (remainingChains-- > 0)
        {
            var next = entityFinder
                .EnemiesCloseTo(currentEnemy.Position, MaxJumpDistance)
                .Where(e => !hitEnemies.Contains(e))
                .OrderBy(e => Vector2.DistanceSquared(
                    e.Position, currentEnemy.Position))
                .FirstOrDefault();

            if (next == null)
                break;

            SpawnVisual(currentEnemy.Position, next.Position);
            next.TakeDamage(owner, damage);

            hitEnemies.Add(next);
            currentEnemy = next;
            damage *= DamageFalloff;
        }
    }
    private void SpawnVisual(Vector2 from, Vector2 to) => spawnEntity.Spawn(new LightningArc(from, to));
}