using System.Collections.Generic;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Entities.Pooling;
using Gameplay.Levelling;

namespace Gameplay.Entities;

public class EntityManager(
    DamageProcessor damageProcessor,
    PickupProcessor pickupProcessor,
    SpatialHashManager spatialHashManager)
    : ISpawnEntity, IEntityFinder, ISpatialHashSources
{
    private readonly List<IEntity> _entitiesToAdd = [];

    private readonly List<EnemyBase> _enemies = new(256);
    private readonly List<Experience> _experiences = [];
    private readonly List<IDamagesEnemies> _damagesEnemies = [];

    private readonly List<IEntity> _entities = [];
    private readonly List<PlayerCharacter> _players = [];
    public IReadOnlyList<IEntity> Entities => _entities;
    public IReadOnlyList<PlayerCharacter> Players => _players;

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
    public IReadOnlyList<EnemyBase> Enemies => _enemies;
    public IReadOnlyList<Experience> Experiences => _experiences;
    public IReadOnlyList<IDamagesEnemies> DamagesEnemies => _damagesEnemies;

    public void Spawn(IEntity entity) => _entitiesToAdd.Add(entity);

    public void Update(GameTime gameTime)
    {
        for (var index = 0; index < _enemies.Count; index++)
        {
            var enemy = _enemies[index];
            var newForce = EnemySeparation.Compute(enemy, spatialHashManager.EnemyNeighborhood);
            enemy.SeparationForce = Vector2.Lerp(enemy.SeparationForce, newForce, 0.25f);
        }

        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            entity.Update(gameTime);
        }

        damageProcessor.ApplyDamage(gameTime, this);
        pickupProcessor.ProcessPickups(_entities);
        RemoveEntities();
        AddPendingEntities();
    }

    private void RemoveEntities()
    {
        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            if (!entity.MarkedForDeletion)
                continue;

            if (entity is EnemyBase enemy)
                _enemies.Remove(enemy);
            if (entity is Experience xp)
                _experiences.Remove(xp);
            if (entity is IDamagesEnemies de)
                _damagesEnemies.Remove(de);
            if (entity is PlayerCharacter p)
                _players.Remove(p);

            _entities.RemoveAt(index);

            if (entity is IPoolableEntity poolable)
                poolable.OnDespawned();
        }
    }

    private void AddPendingEntities()
    {
        foreach (var entity in _entitiesToAdd)
        {
            _entities.Add(entity);
            if (entity is EnemyBase e)
                _enemies.Add(e);
            if (entity is Experience xp)
                _experiences.Add(xp);
            if (entity is IDamagesEnemies d)
                _damagesEnemies.Add(d);
            if (entity is PlayerCharacter p)
                _players.Add(p);
        }

        _entitiesToAdd.Clear();
    }
}