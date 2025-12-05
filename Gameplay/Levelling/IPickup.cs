using Gameplay.CollisionDetection;
using Gameplay.Entities;

namespace Gameplay.Levelling;

internal interface IPickup : ICircleCollider
{
    void OnPickupBy(PlayerCharacter player);
}