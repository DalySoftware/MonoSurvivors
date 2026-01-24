using System.Collections.Generic;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.CollisionDetection;

public class SpatialHashManager
{
    public SpatialHash<EnemyBase> Enemies { get; } = new(64);
    public SpatialHash<IDamagesEnemies> DamagesEnemies { get; } = new(64);
    public SpatialHash<IPickup> Pickups { get; } = new(256);

    public SpatialPointHash<EnemyBase> EnemyNeighborhood { get; } = new(96f);

    public void Update(GameTime gameTime, ISpatialHashSources sources)
    {
        RebuildHash(Enemies, sources.Enemies);
        RebuildHash(DamagesEnemies, sources.DamagesEnemies);
        RebuildHash(Pickups, sources.Experiences);

        RebuildEnemyNeighborhood(sources.Enemies);
    }

    private void RebuildEnemyNeighborhood(IReadOnlyList<EnemyBase> enemies)
    {
        EnemyNeighborhood.Clear();
        for (var i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            EnemyNeighborhood.Insert(e.Position, e);
        }
    }

    private static void RebuildHash<TSource>(SpatialHash<TSource> hash, IReadOnlyList<TSource> sources)
        where TSource : IHasColliders
    {
        hash.Clear();
        foreach (var s in sources)
            hash.Insert(s);
    }
}

public interface ISpatialHashSources
{
    IReadOnlyList<EnemyBase> Enemies { get; }
    IReadOnlyList<IDamagesEnemies> DamagesEnemies { get; }
    IReadOnlyList<Experience> Experiences { get; }
}