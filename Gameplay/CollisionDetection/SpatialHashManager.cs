using System.Collections.Generic;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.CollisionDetection;

public sealed class SpatialHashManager
{
    // Point-insert (one cell per item)
    public SpatialPointHash<EnemyBase> Enemies { get; } = new(64);
    public SpatialPointHash<IDamagesEnemies> DamagesEnemies { get; } = new(64);
    public SpatialPointHash<IPickup> Pickups { get; } = new(256);

    // Point neighborhood for separation (already point-insert)
    public SpatialPointHash<EnemyBase> EnemyNeighborhood { get; } = new(96f);

    // Per-frame maxima for query padding
    public float MaxEnemyRadius { get; private set; }
    public float MaxDamagerRadius { get; private set; }
    public float MaxPickupRadius { get; private set; }

    public void Update(GameTime gameTime, ISpatialHashSources sources)
    {
        MaxEnemyRadius = RebuildPointHash(Enemies, sources.Enemies);
        MaxDamagerRadius = RebuildPointHash(DamagesEnemies, sources.DamagesEnemies);
        MaxPickupRadius = RebuildPointHash(Pickups, sources.Experiences);

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

    private static float RebuildPointHash<TSource>(SpatialPointHash<TSource> hash, IReadOnlyList<TSource> sources)
        where TSource : IHasColliders
    {
        hash.Clear();

        var maxRadius = 0f;

        for (var i = 0; i < sources.Count; i++)
        {
            var s = sources[i];
            var colliders = s.Colliders;

            // Insert once per collider position (snake segments get indexed properly)
            for (var c = 0; c < colliders.Length; c++)
            {
                var col = colliders[c];
                hash.Insert(col.Position, s);

                var r = col.ApproximateRadius;
                if (r > maxRadius) maxRadius = r;
            }
        }

        return maxRadius;
    }
}

public interface ISpatialHashSources
{
    IReadOnlyList<EnemyBase> Enemies { get; }
    IReadOnlyList<IDamagesEnemies> DamagesEnemies { get; }
    IReadOnlyList<Experience> Experiences { get; }
}