using Entities.CollisionDetection;

namespace Entities.Combat;

internal interface IDamageableEnemy : ICircleCollider
{
    public float Health { get; set; }
}