using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

public interface IPickup : IHasColliders
{
    void OnPickupBy(PlayerCharacter player);
}