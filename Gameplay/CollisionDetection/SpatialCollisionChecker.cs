using System.Collections.Generic;
using System.Linq;

namespace Gameplay.CollisionDetection;

/// <summary>
///     Performs efficient collision checking using spatial hashing to avoid O(n×m) full scans.
/// </summary>
internal class SpatialCollisionChecker(float cellSize = 50f)
{
    /// <summary>
    ///     Finds all overlapping pairs between two collections using spatial hashing.
    /// </summary>
    public IEnumerable<(TTarget target, TSource source)> FindOverlaps<TTarget, TSource>(
        IEnumerable<TTarget> targets,
        IEnumerable<TSource> sources)
        where TTarget : IHasColliders
        where TSource : IHasColliders
    {
        var spatialHash = new SpatialHash<TSource>(cellSize);

        foreach (var source in sources)
            spatialHash.Insert(source);

        // For each target collider, query nearby sources and check overlap
        foreach (var target in targets)
        foreach (var targetCollider in target.Colliders)
        {
            var radius = targetCollider.ApproximateRadius;
            var cellRadius = (int)(radius / spatialHash.CellSize) + 1;

            foreach (var source in spatialHash.QueryNearby(targetCollider.Position, cellRadius))
                if (source.Colliders.Any(c => CollisionChecker.HasOverlap(c, targetCollider)))
                    yield return (target, source);
        }
    }
}