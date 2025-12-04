using Gameplay.CollisionDetection;

namespace Gameplay.Combat;

internal interface IDamagesPlayer : ICircleCollider
{
    public float Damage { get; }
}