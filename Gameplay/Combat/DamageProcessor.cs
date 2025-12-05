using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Combat;

/// <summary>
///     Processes damage for a fixed state of entities, ie for any relevant overlapping entities
/// </summary>
public static class DamageProcessor
{
    public static void ApplyDamage(IReadOnlyCollection<IEntity> entities)
    {
        var playerDamagers = entities.OfType<IDamagesPlayer>();
        var players = entities.OfType<IDamageablePlayer>();

        var playerDamagePairs = playerDamagers
            .SelectMany(damager => players.Select(player => (player, damager)))
            .Where(pair => CircleChecker.HasOverlap(pair.damager, pair.player));

        foreach (var (damageablePlayer, damager) in playerDamagePairs)
            damageablePlayer.Health -= damager.Damage;

        var enemyDamagers = entities.OfType<IDamagesEnemies>();
        var enemies = entities.OfType<IDamageableEnemy>();

        var enemyDamagePairs = enemyDamagers
            .SelectMany(damager => enemies.Select(enemy => (enemy, damager)))
            .Where(pair => CircleChecker.HasOverlap(pair.damager, pair.enemy));

        foreach (var (damageableEnemy, damager) in enemyDamagePairs)
        {
            damageableEnemy.Health -= damager.Damage;
            damager.OnHit();
        }
    }
}