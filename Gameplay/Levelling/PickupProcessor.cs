using System.Collections.Generic;
using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public class PickupProcessor(SpatialCollisionChecker collisionChecker, SpatialHashManager spatialHashManager)
{
    private const int CellSize = 256;

    private readonly List<IPickup> _nearbyPickups = new(256);

    private readonly List<(PlayerCharacter player, IPickup pickup)> _pickupOverlaps = new(256);
    private readonly HashSet<IPickup> _alreadyUsed = [];

    private readonly List<PlayerCharacter> _playersScratch = new(4);

    internal void ProcessPickups(IReadOnlyList<IEntity> entities)
    {
        ProcessExperienceAttraction(entities);

        _alreadyUsed.Clear();
        _pickupOverlaps.Clear();

        // Build concrete lists without LINQ/boxing
        _playersScratch.Clear();

        for (var i = 0; i < entities.Count; i++)
        {
            var e = entities[i];

            if (e is PlayerCharacter player) _playersScratch.Add(player);
        }

        collisionChecker.FindOverlapsWithPickups(_playersScratch, _pickupOverlaps);

        for (var i = 0; i < _pickupOverlaps.Count; i++)
        {
            var (player, pickup) = _pickupOverlaps[i];
            if (_alreadyUsed.Add(pickup))
                pickup.OnPickupBy(player);
        }
    }


    private void ProcessExperienceAttraction(IReadOnlyList<IEntity> entities)
    {
        for (var i = 0; i < entities.Count; i++)
            if (entities[i] is PlayerCharacter player)
                AttractExperience(player);
    }


    private void AttractExperience(PlayerCharacter player)
    {
        const float attractionRange = 1000f;
        const float attractionRangeSq = attractionRange * attractionRange;
        const int cellRadius = (int)(attractionRange / CellSize) + 1;

        spatialHashManager.Pickups.QueryNearbyInto(player.Position, _nearbyPickups, cellRadius);

        foreach (var pickup in _nearbyPickups)
        {
            if (pickup is not Experience experience) continue;

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