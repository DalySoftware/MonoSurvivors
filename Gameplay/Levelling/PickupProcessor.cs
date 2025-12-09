using System.Collections.Generic;
using System.Linq;
using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public class PickupProcessor()
{
    private readonly SpatialHash<IPickup> _spatialHash = new(50f);

    public void ProcessPickups(IReadOnlyCollection<IEntity> entities)
    {
        var pickups = entities.OfType<IPickup>();
        var players = entities.OfType<PlayerCharacter>();
        var alreadyUsed = new HashSet<IPickup>();

        _spatialHash.Clear();
        foreach (var pickup in pickups)
            _spatialHash.Insert(pickup);

        var playerPairs = players
            .SelectMany(player => _spatialHash.QueryNearby(player.Position).Select(pickup => (pickup, player)));
        
        foreach (var (pickup, player) in playerPairs)
        {
            if (alreadyUsed.Contains(pickup) || 
                !CircleChecker.HasOverlap(pickup, player))
                continue;

            alreadyUsed.Add(pickup);
            pickup.OnPickupBy(player);
        }
    }
}