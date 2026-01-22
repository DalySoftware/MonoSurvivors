using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;
using Gameplay.Entities.Enemies;
using Gameplay.Telemetry;

namespace Gameplay.Combat;

/// <summary>
///     Processes damage for a fixed state of entities, ie for any relevant overlapping entities
/// </summary>
internal class DamageProcessor(PerformanceMetrics perf)
{
    private readonly SpatialCollisionChecker _collisionChecker = new(perf);

    private readonly List<(IDamageablePlayer player, IDamagesPlayer damager)> _playerHits = new(256);
    private readonly List<(IDamagesEnemies damager, EnemyBase enemy)> _enemyHits = new(256);


    internal void ApplyDamage(GameTime gameTime, IReadOnlyCollection<IEntity> entities)
    {
        var playerDamagers = entities.OfType<IDamagesPlayer>();
        var players = entities
            .OfType<IDamageablePlayer>()
            .Where(p => p.Damageable);

        var damagerHash = _collisionChecker.BuildHash(playerDamagers);

        _collisionChecker.FindOverlapsInto(players, damagerHash, _playerHits);

        for (var i = 0; i < _playerHits.Count; i++)
        {
            var (player, damager) = _playerHits[i];
            player.TakeDamage(damager.Damage);
        }

        var enemyDamagers = entities.OfType<IDamagesEnemies>();
        var enemies = entities.OfType<EnemyBase>();

        var enemyHash = _collisionChecker.BuildHash(enemies);

        _collisionChecker.FindOverlapsInto(enemyDamagers, enemyHash, _enemyHits);

        for (var i = 0; i < _enemyHits.Count; i++)
        {
            var (damager, enemy) = _enemyHits[i];
            damager.OnHit(gameTime, enemy);
        }
    }
}