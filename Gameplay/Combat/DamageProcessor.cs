using System.Collections.Generic;
using Gameplay.CollisionDetection;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;

namespace Gameplay.Combat;

/// <summary>
///     Processes damage for a fixed state of entities, ie for any relevant overlapping entities
/// </summary>
public class DamageProcessor(
    SpatialCollisionChecker collisionChecker)
{
    private readonly List<(IDamageablePlayer player, IDamagesPlayer damager)> _playerHits = new(256);
    private readonly List<(EnemyBase enemy, IDamagesEnemies damager)> _enemyHits = new(256);

    internal void ApplyDamage(GameTime gameTime, EntityManager  entityManager)
    {
        collisionChecker.FindOverlapsWithPlayerDamagers(entityManager.Players, _playerHits);

        for (var i = 0; i < _playerHits.Count; i++)
        {
            var (player, damager) = _playerHits[i];
            player.TakeDamage(damager.Damage);
        }

        collisionChecker.FindOverlapsWithEnemyDamagers(entityManager.Enemies, _enemyHits);
        for (var i = 0; i < _enemyHits.Count; i++)
        {
            var (enemy, damager) = _enemyHits[i];
            damager.OnHit(gameTime, enemy);
        }
    }
}