using Entities.CollisionDetection;

namespace Entities.Levelling;

internal interface IPickup : ICircleCollider
{
    void OnPickupBy(PlayerCharacter player);
}