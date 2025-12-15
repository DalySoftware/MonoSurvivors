using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat;

/// <summary>
///     Processes damage for a fixed state of entities, ie for any relevant overlapping entities
/// </summary>
internal class DamageProcessor
{
    private readonly SpatialCollisionChecker _collisionChecker = new();

    internal void ApplyDamage(IReadOnlyCollection<IEntity> entities)
    {
        var playerDamagers = entities.OfType<IDamagesPlayer>();
        var players = entities
            .OfType<IDamageablePlayer>()
            .Where(p => p.Damageable);

        foreach (var (player, damager) in _collisionChecker.FindOverlaps(players, playerDamagers))
            player.TakeDamage(damager.Damage);

        var enemyDamagers = entities.OfType<IDamagesEnemies>();
        var enemies = entities.OfType<EnemyBase>();

        foreach (var (enemy, damager) in _collisionChecker.FindOverlaps(enemies, enemyDamagers))
            damager.OnHit(enemy);
    }
}