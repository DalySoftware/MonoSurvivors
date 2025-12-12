using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

internal interface IPickup : IHasCollider
{
    void OnPickupBy(PlayerCharacter player);
}