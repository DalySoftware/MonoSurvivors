using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamagesPlayer : IHasCollider
{
    public int Damage { get; }
}