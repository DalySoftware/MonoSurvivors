using Entities.CollisionDetection;

namespace Entities.Combat;

internal interface IDamagesPlayer : ICircleCollider
{
    public float Damage { get; }
}