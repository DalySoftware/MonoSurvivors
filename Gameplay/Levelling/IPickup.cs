using Gameplay.CollisionDetection;

namespace Gameplay.Levelling;

internal interface IPickup : ICircleCollider
{
    void OnPickupBy(PlayerCharacter player);
}