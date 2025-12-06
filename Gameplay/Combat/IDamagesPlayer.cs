using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamagesPlayer : ICircleCollider
{
    public int Damage { get; }
}