using System.Collections.Generic;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.CollisionDetection;

public sealed class SpatialCollisionChecker(SpatialHashManager hashManager)
{
    public void FindOverlapsWithEnemies<TTarget>(
        IReadOnlyList<TTarget> targets,
        List<(TTarget target, EnemyBase source)> results)
        where TTarget : IHasColliders
        => FindOverlapsInto(targets, hashManager.Enemies, hashManager.MaxEnemyRadius, results);

    public void FindOverlapsWithEnemyDamagers(
        IReadOnlyList<EnemyBase> enemies,
        List<(EnemyBase target, IDamagesEnemies source)> results)
        => FindOverlapsInto(enemies, hashManager.DamagesEnemies, hashManager.MaxDamagerRadius, results);

    public void FindOverlapsWithPickups<TTarget>(
        IReadOnlyList<TTarget> targets,
        List<(TTarget target, IPickup source)> results)
        where TTarget : IHasColliders
        => FindOverlapsInto(targets, hashManager.Pickups, hashManager.MaxPickupRadius, results);

    private void FindOverlapsInto<TTarget, TSource>(
        IReadOnlyList<TTarget> targets,
        SpatialPointHash<TSource> hash,
        float maxSourceRadius,
        List<(TTarget target, TSource source)> results)
        where TTarget : IHasColliders
        where TSource : class, IHasColliders
    {
        results.Clear();
        var nearby = NearbyScratch<TSource>.List;
        var seenMulti = SeenMultiScratch<TSource>.List;

        for (var t = 0; t < targets.Count; t++)
        {
            var target = targets[t];
            var targetColliders = target.Colliders;

            for (var tc = 0; tc < targetColliders.Length; tc++)
            {
                var targetCollider = targetColliders[tc];

                var padded = targetCollider.ApproximateRadius + maxSourceRadius;
                var cellRadius = (int)(padded / hash.CellSize) + 1;

                nearby.Clear();
                seenMulti.Clear();

                hash.QueryNearbyInto(targetCollider.Position, nearby, cellRadius);

                for (var i = 0; i < nearby.Count; i++)
                {
                    var source = nearby[i];
                    var sourceColliders = source.Colliders;

                    // Only needed for multi-collider sources (your boss)
                    if (sourceColliders.Length > 1)
                    {
                        if (WasSeen(seenMulti, source))
                            continue;
                        seenMulti.Add(source);
                    }

                    for (var sc = 0; sc < sourceColliders.Length; sc++)
                    {
                        if (!CollisionChecker.HasOverlap(sourceColliders[sc], targetCollider))
                            continue;

                        results.Add((target, source));
                        break;
                    }
                }
            }
        }

        return;

        static bool WasSeen(List<TSource> list, TSource item)
        {
            for (var i = 0; i < list.Count; i++)
                if (ReferenceEquals(list[i], item))
                    return true;
            return false;
        }
    }

    private static class SeenMultiScratch<T>
    {
        public readonly static List<T> List = new(8);
    }

    private static class NearbyScratch<T>
    {
        public readonly static List<T> List = new(256);
    }
}