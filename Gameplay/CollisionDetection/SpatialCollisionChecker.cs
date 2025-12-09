using System.Collections.Generic;

namespace Gameplay.CollisionDetection;

/// <summary>
/// Performs efficient collision checking using spatial hashing to avoid O(n×m) full scans.
/// </summary>
internal class SpatialCollisionChecker(float cellSize = 50f)
{
    /// <summary>
    /// Finds all overlapping pairs between two collections using spatial hashing.
    /// </summary>
    public IEnumerable<(TTarget target, TSource source)> FindOverlaps<TTarget, TSource>(
        IEnumerable<TTarget> targets,
        IEnumerable<TSource> sources)
        where TTarget : ICircleCollider
        where TSource : ICircleCollider
    {
        var spatialHash = new SpatialHash<TSource>(cellSize);

        foreach (var source in sources)
            spatialHash.Insert(source);

        foreach (var target in targets)
        {
            foreach (var source in spatialHash.QueryNearby(target.Position))
            {
                if (CircleChecker.HasOverlap(target, source))
                    yield return (target, source);
            }
        }
    }
}
