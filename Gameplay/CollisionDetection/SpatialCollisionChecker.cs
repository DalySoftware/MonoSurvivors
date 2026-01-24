using System.Collections.Generic;
using Gameplay.Combat;
using Gameplay.Entities.Enemies;
using Gameplay.Levelling;

namespace Gameplay.CollisionDetection;

public sealed class SpatialCollisionChecker(SpatialHashManager hashManager)
{
    public void FindOverlapsWithEnemies<TTarget>(
        IEnumerable<TTarget> targets,
        List<(TTarget target, EnemyBase source)> results)
        where TTarget : IHasColliders
        => FindOverlapsInto(targets, hashManager.Enemies, results);

    public void FindOverlapsWithPlayerDamagers(IEnumerable<IDamageablePlayer> players,
        List<(IDamageablePlayer target, IDamagesPlayer source)> results)
        => FindOverlapsInto(players, hashManager.DamagesPlayers, results);

    public void FindOverlapsWithEnemyDamagers(IEnumerable<EnemyBase> enemies,
        List<(EnemyBase target, IDamagesEnemies source)> results)
        => FindOverlapsInto(enemies, hashManager.DamagesEnemies, results);

    public void FindOverlapsWithPickups<TTarget>(
        IEnumerable<TTarget> targets,
        List<(TTarget target, IPickup source)> results)
        where TTarget : IHasColliders
        => FindOverlapsInto(targets, hashManager.Pickups, results);

    private void FindOverlapsInto<TTarget, TSource>(
        IEnumerable<TTarget> targets,
        SpatialHash<TSource> hash,
        List<(TTarget target, TSource source)> results)
        where TTarget : IHasColliders
        where TSource : IHasColliders
    {
        results.Clear();
        var nearby = NearbyScratch<TSource>.List;

        foreach (var target in targets)
        foreach (var targetCollider in target.Colliders)
        {
            var cellRadius = (int)(targetCollider.ApproximateRadius / hash.CellSize) + 1;

            nearby.Clear(); // don’t rely on QueryNearbyInto clearing
            hash.QueryNearbyInto(targetCollider.Position, nearby, cellRadius);

            for (var i = 0; i < nearby.Count; i++)
            {
                var source = nearby[i];

                foreach (var sourceCollider in source.Colliders)
                {
                    if (!CollisionChecker.HasOverlap(sourceCollider, targetCollider))
                        continue;

                    results.Add((target, source));
                    break; // stop checking this source’s remaining colliders
                }
            }
        }
    }

    // One scratch list per TSource (shared). Fast and no casts.
    private static class NearbyScratch<T>
    {
        public readonly static List<T> List = new(256);
    }
}