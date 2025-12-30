using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamagesPlayer : IHasColliders
{
    public int Damage { get; }
}