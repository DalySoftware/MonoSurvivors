using System.Collections.Generic;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Entities.Pooling;
using Gameplay.Levelling;

namespace Gameplay.Entities;

public class EntityManager(DamageProcessor damageProcessor, PickupProcessor pickupProcessor)
    : ISpawnEntity, IEntityFinder
{
    private readonly List<IEntity> _entitiesToAdd = [];
    private readonly SpatialHash<EnemyBase> _spatialHash = new(64);

    private readonly List<EnemyBase> _enemies = new(256);

    private readonly List<IEntity> _entities = [];
    public IReadOnlyList<IEntity> Entities => _entities;

    public EnemyBase? NearestEnemyTo(IHasPosition source)
    {
        EnemyBase? best = null;
        var bestD = float.PositiveInfinity;

        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            if (entity is EnemyBase enemy)
            {
                var distanceSquared = Vector2.DistanceSquared(source.Position, enemy.Position);
                if (distanceSquared < bestD)
                {
                    bestD = distanceSquared;
                    best = enemy;
                }
            }
        }

        return best;
    }

    public IEnumerable<EnemyBase> EnemiesCloseTo(Vector2 position, float maxDistance)
    {
        var maxDistSq = maxDistance * maxDistance;

        // _enemies is rebuilt once per frame in Update(), and not modified during entity updates.
        for (var i = 0; i < _enemies.Count; i++)
        {
            var e = _enemies[i];
            if (Vector2.DistanceSquared(e.Position, position) <= maxDistSq)
                yield return e;
        }
    }

    public void Spawn(params IEnumerable<IEntity> entities) => _entitiesToAdd.AddRange(entities);

    public void Update(GameTime gameTime)
    {
        _enemies.Clear();
        for (var index = 0; index < _entities.Count; index++)
        {
            var x = _entities[index];
            if (x is EnemyBase e)
                _enemies.Add(e);
        }


        _spatialHash.Clear();
        for (var index = 0; index < _enemies.Count; index++)
        {
            var enemy = _enemies[index];
            _spatialHash.Insert(enemy);
        }

        for (var index = 0; index < _enemies.Count; index++)
        {
            var enemy = _enemies[index];
            _spatialHash.QueryNearbyInto(enemy.Position, enemy.NearbyEnemies);
        }

        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            entity.Update(gameTime);
        }

        damageProcessor.ApplyDamage(gameTime, _entities);
        pickupProcessor.ProcessPickups(_entities);
        RemoveEntities();
        AddPendingEntities();
    }

    private void RemoveEntities()
    {
        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            if (entity.MarkedForDeletion && entity is IPoolableEntity poolable)
                poolable.OnDespawned();
        }

        _entities.RemoveAll(e => e.MarkedForDeletion);
    }

    private void AddPendingEntities()
    {
        _entities.AddRange(_entitiesToAdd);
        _entitiesToAdd.Clear();
    }
}