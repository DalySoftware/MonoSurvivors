using Entities.CollisionDetection;

namespace Entities.Combat;

internal interface IDamageablePlayer : ICircleCollider
{
    public float Health { get; set; }
}