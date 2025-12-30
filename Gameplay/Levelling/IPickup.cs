using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

internal interface IPickup : IHasColliders
{
    void OnPickupBy(PlayerCharacter player);
}