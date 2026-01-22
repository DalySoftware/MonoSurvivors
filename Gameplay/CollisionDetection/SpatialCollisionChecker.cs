using System;
using System.Collections.Generic;
using Gameplay.Telemetry;

namespace Gameplay.CollisionDetection;

internal sealed class SpatialCollisionChecker(PerformanceMetrics perf, float cellSize = 50f)
{
    private readonly Dictionary<Type, object> _hashCache = new();
    private readonly Dictionary<Type, object> _nearbyScratchCache = new();

    private SpatialHash<T> GetHash<T>() where T : IHasColliders
    {
        if (_hashCache.TryGetValue(typeof(T), out var existing))
            return (SpatialHash<T>)existing;

        var created = new SpatialHash<T>(cellSize, perf);
        _hashCache.Add(typeof(T), created);
        return created;
    }

    public SpatialHash<TSource> BuildHash<TSource>(IEnumerable<TSource> sources)
        where TSource : IHasColliders
    {
        var hash = GetHash<TSource>();
        hash.Clear();

        foreach (var s in sources)
            hash.Insert(s);

        return hash;
    }

    private List<T> GetNearbyScratch<T>()
    {
        if (_nearbyScratchCache.TryGetValue(typeof(T), out var existing))
            return (List<T>)existing;

        var created = new List<T>(256);
        _nearbyScratchCache.Add(typeof(T), created);
        return created;
    }

    public void FindOverlapsInto<TTarget, TSource>(
        IEnumerable<TTarget> targets,
        SpatialHash<TSource> hash,
        List<(TTarget target, TSource source)> results)
        where TTarget : IHasColliders
        where TSource : IHasColliders
    {
        results.Clear();
        var nearby = GetNearbyScratch<TSource>();

        foreach (var target in targets)
        foreach (var targetCollider in target.Colliders)
        {
            var radius = targetCollider.ApproximateRadius;
            var cellRadius = (int)(radius / hash.CellSize) + 1;

            hash.QueryNearbyInto(targetCollider.Position, nearby, cellRadius);

            for (var i = 0; i < nearby.Count; i++)
            {
                var source = nearby[i];

                foreach (var c in source.Colliders)
                    if (CollisionChecker.HasOverlap(c, targetCollider))
                    {
                        results.Add((target, source));
                        break;
                    }
            }
        }
    }
}