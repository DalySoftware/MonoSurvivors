using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

internal class PickupProcessor
{
    private readonly SpatialCollisionChecker _collisionChecker = new();

    internal void ProcessPickups(IReadOnlyCollection<IEntity> entities)
    {
        var pickups = entities.OfType<IPickup>();
        var players = entities.OfType<PlayerCharacter>();
        var alreadyUsed = new HashSet<IPickup>();

        foreach (var (player, pickup) in _collisionChecker.FindOverlaps(players, pickups))
            if (alreadyUsed.Add(pickup))
                pickup.OnPickupBy(player);
    }
}