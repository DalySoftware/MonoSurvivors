using System.Collections.Generic;
using System.Linq;
using Gameplay.Behaviour;
using Gameplay.CollisionDetection;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.Entities;

public class EntityManager : ISpawnEntity, IEntityFinder
{
    private readonly DamageProcessor _damageProcessor = new();
    private readonly List<IEntity> _entitiesToAdd = [];
    private readonly PickupProcessor _pickupProcessor = new();
    private SpatialHash<EnemyBase> _spatialHash = new(64f);
    public List<IEntity> Entities { get; } = [];

    private IEnumerable<EnemyBase> Enemies => Entities.OfType<EnemyBase>();

    public EnemyBase? NearestEnemyTo(IHasPosition source) =>
        Enemies.MinBy(e => Vector2.DistanceSquared(source.Position, e.Position));

    public IEnumerable<EnemyBase> EnemiesCloseTo(Vector2 position, float maxDistance) => _spatialHash
        .QueryNearby(position)
        .Where(e => Vector2.DistanceSquared(e.Position, position) <= maxDistance * maxDistance);


    public void Spawn(params IEnumerable<IEntity> entities) => _entitiesToAdd.AddRange(entities);

    public void Update(GameTime gameTime)
    {
        var enemies = Enemies.ToList();

        _spatialHash = new SpatialHash<EnemyBase>(64f);
        foreach (var enemy in enemies)
            _spatialHash.Insert(enemy);

        foreach (var enemy in enemies)
            enemy.NearbyEnemies = _spatialHash.QueryNearby(enemy.Position);

        foreach (var entity in Entities)
            entity.Update(gameTime);

        _damageProcessor.ApplyDamage(Entities);
        _pickupProcessor.ProcessPickups(Entities);
        RemoveEntities();
        AddPendingEntities();
    }

    private void RemoveEntities() => Entities.RemoveAll(e => e.MarkedForDeletion);

    private void AddPendingEntities()
    {
        Entities.AddRange(_entitiesToAdd);
        _entitiesToAdd.Clear();
    }
}