using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public static class PickupProcessor
{
    public static void ProcessPickups(IReadOnlyCollection<IEntity> entities)
    {
        var pickups = entities.OfType<IPickup>();
        var players = entities.OfType<PlayerCharacter>();
        var alreadyUsed = new HashSet<IPickup>();

        var playerPairs = pickups
            .SelectMany(pickup => players.Select(player => (pickup, player)))
            .Where(pair => CircleChecker.HasOverlap(pair.pickup, pair.player));

        foreach (var (pickup, player) in playerPairs)
            if (alreadyUsed.Add(pickup))
                pickup.OnPickupBy(player);
    }
}