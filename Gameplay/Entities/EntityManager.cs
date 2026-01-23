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
    private readonly List<EnemyBase> _nearbyEnemies = new(256);
    private readonly List<EnemyBase> _closeEnemies = new(256);

    public List<IEntity> Entities { get; } = [];

    public EnemyBase? NearestEnemyTo(IHasPosition source)
    {
        EnemyBase? best = null;
        var bestD = float.PositiveInfinity;

        for (var index = 0; index < Entities.Count; index++)
        {
            var entity = Entities[index];
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

    public IReadOnlyCollection<EnemyBase> EnemiesCloseTo(Vector2 position, float maxDistance)
    {
        _closeEnemies.Clear();

        var maxDistSq = maxDistance * maxDistance;
        var cellRadius = (int)(maxDistance / _spatialHash.CellSize) + 1;

        _spatialHash.QueryNearbyInto(position, _nearbyEnemies, cellRadius);

        for (var index = 0; index < _nearbyEnemies.Count; index++)
        {
            var e = _nearbyEnemies[index];
            if (Vector2.DistanceSquared(e.Position, position) <= maxDistSq)
                _closeEnemies.Add(e);
        }

        return _closeEnemies;
    }

    public void Spawn(params IEnumerable<IEntity> entities) => _entitiesToAdd.AddRange(entities);

    public void Update(GameTime gameTime)
    {
        _enemies.Clear();
        for (var index = 0; index < Entities.Count; index++)
        {
            var x = Entities[index];
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

        for (var index = 0; index < Entities.Count; index++)
        {
            var entity = Entities[index];
            entity.Update(gameTime);
        }

        damageProcessor.ApplyDamage(gameTime, Entities);
        pickupProcessor.ProcessPickups(Entities);
        RemoveEntities();
        AddPendingEntities();
    }

    private void RemoveEntities()
    {
        for (var index = 0; index < Entities.Count; index++)
        {
            var entity = Entities[index];
            if (entity.MarkedForDeletion && entity is IPoolableEntity poolable)
                poolable.OnDespawned();
        }

        Entities.RemoveAll(e => e.MarkedForDeletion);
    }

    private void AddPendingEntities()
    {
        Entities.AddRange(_entitiesToAdd);
        _entitiesToAdd.Clear();
    }
}