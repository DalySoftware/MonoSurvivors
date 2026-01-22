using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

internal class PickupProcessor
{
    private readonly SpatialCollisionChecker _collisionChecker = new();
    private readonly SpatialHash<Experience> _experienceHash = new(64f);

    internal void ProcessPickups(IReadOnlyCollection<IEntity> entities)
    {
        ProcessExperienceAttraction(entities);

        var pickups = entities.OfType<IPickup>();
        var players = entities.OfType<PlayerCharacter>();
        var alreadyUsed = new HashSet<IPickup>();

        foreach (var (player, pickup) in _collisionChecker.FindOverlaps(players, pickups))
            if (alreadyUsed.Add(pickup))
                pickup.OnPickupBy(player);
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
        var cellRadius = (int)(attractionRange / _experienceHash.CellSize) + 1;
        foreach (var experience in _experienceHash.QueryNearby(player.Position, cellRadius))
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