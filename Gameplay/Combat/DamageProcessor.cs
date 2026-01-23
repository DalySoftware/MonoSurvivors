using System.Collections.Generic;
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

    private readonly List<IDamagesPlayer> _playerDamagers = new(256);
    private readonly List<IDamageablePlayer> _players = new(16);

    private readonly List<IDamagesEnemies> _enemyDamagers = new(256);
    private readonly List<EnemyBase> _enemies = new(256);

    private readonly List<(IDamageablePlayer player, IDamagesPlayer damager)> _playerHits = new(256);
    private readonly List<(IDamagesEnemies damager, EnemyBase enemy)> _enemyHits = new(256);

    internal void ApplyDamage(GameTime gameTime, IReadOnlyCollection<IEntity> entities)
    {
        _playerDamagers.Clear();
        _players.Clear();
        _enemyDamagers.Clear();
        _enemies.Clear();

        foreach (var e in entities)
        {
            if (e is IDamagesPlayer dp) _playerDamagers.Add(dp);
            if (e is IDamageablePlayer { Damageable: true } p) _players.Add(p);
            if (e is IDamagesEnemies de) _enemyDamagers.Add(de);
            if (e is EnemyBase enemy) _enemies.Add(enemy);
        }

        var damagerHash = _collisionChecker.BuildHash(_playerDamagers);
        _collisionChecker.FindOverlapsInto(_players, damagerHash, _playerHits);

        for (var i = 0; i < _playerHits.Count; i++)
        {
            var (player, damager) = _playerHits[i];
            player.TakeDamage(damager.Damage);
        }

        var enemyHash = _collisionChecker.BuildHash(_enemies);
        _collisionChecker.FindOverlapsInto(_enemyDamagers, enemyHash, _enemyHits);

        for (var i = 0; i < _enemyHits.Count; i++)
        {
            var (damager, enemy) = _enemyHits[i];
            damager.OnHit(gameTime, enemy);
        }
    }
}