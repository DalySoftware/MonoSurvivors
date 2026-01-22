using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;
using Gameplay.Telemetry;

namespace Gameplay.Levelling;

internal class PickupProcessor(PerformanceMetrics perf)
{
    private const int CellSize = 256;
    private readonly SpatialCollisionChecker _collisionChecker = new(perf);
    private readonly SpatialHash<Experience> _experienceHash = new(CellSize, perf);

    private readonly List<Experience> _nearbyExperience = new(256);

    private readonly List<(PlayerCharacter player, IPickup pickup)> _pickupOverlaps = new(256);
    private readonly HashSet<IPickup> _alreadyUsed = [];

    internal void ProcessPickups(IReadOnlyCollection<IEntity> entities)
    {
        ProcessExperienceAttraction(entities);

        var pickups = entities.OfType<IPickup>();
        var players = entities.OfType<PlayerCharacter>();

        _alreadyUsed.Clear();

        var pickupHash = _collisionChecker.BuildHash(pickups);

        _collisionChecker.FindOverlapsInto(players, pickupHash, _pickupOverlaps);

        for (var i = 0; i < _pickupOverlaps.Count; i++)
        {
            var (player, pickup) = _pickupOverlaps[i];
            if (_alreadyUsed.Add(pickup))
                pickup.OnPickupBy(player);
        }
    }

    private void ProcessExperienceAttraction(IReadOnlyCollection<IEntity> entities)
    {
        _experienceHash.Clear();

        var experiences = entities.OfType<Experience>();
        foreach (var xp in experiences)
            _experienceHash.Insert(xp);

        foreach (var player in entities.OfType<PlayerCharacter>())
            AttractExperience(player);
    }

    private void AttractExperience(PlayerCharacter player)
    {
        const float attractionRange = 1000f;
        const float attractionRangeSq = attractionRange * attractionRange;
        const int cellRadius = (int)(attractionRange / CellSize) + 1;

        _experienceHash.QueryNearbyInto(player.Position, _nearbyExperience, cellRadius);

        foreach (var experience in _nearbyExperience)
        {
            var delta = player.Position - experience.Position;
            var distSq = delta.LengthSquared();

            if (distSq > attractionRangeSq)
                continue;

            const float baseAttractSpeed = 60f;
            var direction = Vector2.Normalize(delta);
            var speed = baseAttractSpeed / distSq;
            experience.AddVelocity(direction * speed);
        }
    }
}